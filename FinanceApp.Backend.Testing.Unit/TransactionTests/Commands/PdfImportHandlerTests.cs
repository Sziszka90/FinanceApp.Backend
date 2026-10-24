using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Dtos.BankStatementPromptDtos;
using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Hubs;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.ImportPdf;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FinanceApp.Backend.Domain.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Commands;

public class PdfImportHandlerTests : TestBase
{
  private readonly User _user = new(null, "test-user", "test@example.com", true, "hash", CurrencyEnum.EUR);
  private readonly Mock<IRepository<BankStatementPrompt>> _prompts = new();
  private readonly Mock<IRepository<PdfImportRecord>> _pdfImports = new();
  private readonly Mock<ISignalRService> _signalR = new();
  private readonly Mock<IPdfImportStatusStore> _importStatuses = new();
  private readonly RabbitMqSettings _settings = new()
  {
    PdfRequestQueue = "pdf-requests",
    RequestQueue = "match-requests"
  };

  public PdfImportHandlerTests()
  {
    _pdfImports.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
      .ReturnsAsync((PdfImportRecord?)null);
    _pdfImports.Setup(repository => repository.CreateAsync(It.IsAny<PdfImportRecord>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((PdfImportRecord record, CancellationToken _) => record);
    UserServiceMock.Setup(service => service.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result<User>.Success(_user));
    _importStatuses.Setup(store => store.SetAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(),
        It.IsAny<string?>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
  }

  [Fact]
  public async Task ImportPdf_ValidPdfPublishesJobWithSelectedPrompt()
  {
    var selectedPrompt = new BankStatementPrompt
    {
      UserId = _user.Id,
      Name = "My Bank",
      Instructions = "Extract posted transactions",
      IsSelected = true
    };
    _prompts.Setup(repository => repository.GetFirstOrDefaultAsync(
        It.IsAny<System.Linq.Expressions.Expression<Func<BankStatementPrompt, bool>>>(), true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(selectedPrompt);
    var handler = new ImportPdfCommandHandler(UserServiceMock.Object, _importStatuses.Object, RabbitMqClientMock.Object, _prompts.Object,
      Options.Create(_settings));
    var correlationId = Guid.NewGuid().ToString();
    string? publishedQueue = null;
    string? publishedMessage = null;
    RabbitMqClientMock.Setup(client => client.PublishToQueueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Callback<string, string, CancellationToken>((queue, message, _) => { publishedQueue = queue; publishedMessage = message; })
      .Returns(Task.CompletedTask);

    var result = await handler.Handle(new ImportPdfCommand(CreateUpload("statement.pdf", "%PDF-1.7 test", correlationId)), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(correlationId, result.Data);
    Assert.Equal(_settings.PdfRequestQueue, publishedQueue);
    _importStatuses.Verify(store => store.SetAsync(_user.Email, Guid.Parse(correlationId), "queued", null,
      It.IsAny<CancellationToken>()), Times.Once);
    var job = JsonSerializer.Deserialize<PdfImportJobDto>(publishedMessage!)!;
    Assert.Equal(_user.Id.ToString(), job.UserId);
    Assert.Equal("statement.pdf", job.Filename);
    Assert.Equal(Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("%PDF-1.7 test")), job.PdfBase64);
    Assert.Equal(selectedPrompt.Id.ToString(), job.PromptId);
    Assert.Equal("My Bank", job.PromptName);
    Assert.Equal("Extract posted transactions", job.Prompt);
  }

  [Fact]
  public async Task ImportPdf_InvalidPdfHeaderReturnsFailureWithoutPublishing()
  {
    var handler = new ImportPdfCommandHandler(UserServiceMock.Object, _importStatuses.Object, RabbitMqClientMock.Object, _prompts.Object,
      Options.Create(_settings));

    var result = await handler.Handle(new ImportPdfCommand(CreateUpload("statement.pdf", "not a pdf", Guid.NewGuid().ToString())),
      CancellationToken.None);

    Assert.False(result.IsSuccess);
    RabbitMqClientMock.Verify(client => client.PublishToQueueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
      Times.Never);
  }

  [Fact]
  public async Task CompletePdfImport_ValidStatementPersistsAndQueuesGrouping()
  {
    var group = new TransactionGroup("Food", "Food spending", "food", _user);
    var handler = CreateCompleteHandler();
    UserRepositoryMock.Setup(repository => repository.GetByIdAsync(_user.Id, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(_user);
    TransactionGroupRepositoryMock.Setup(repository => repository.GetAllByUserIdAsync(_user.Id, true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroup> { group });
    List<Transaction>? saved = null;
    TransactionRepositoryMock.Setup(repository => repository.BatchCreateTransactionsAsync(It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()))
      .Callback<List<Transaction>, CancellationToken>((transactions, _) => saved = transactions)
      .ReturnsAsync((List<Transaction> transactions, CancellationToken _) => transactions);
    RabbitMQClientMockSetup();

    var correlationId = Guid.NewGuid().ToString();
    var result = await handler.Handle(new CompletePdfImportCommand(new PdfImportResultDto
    {
      CorrelationId = correlationId,
      UserId = _user.Id.ToString(),
      Success = true,
      Data = new PdfStatementDto
      {
        StatementCurrency = "EUR",
        OpeningBalance = 100m,
        ClosingBalance = 75m,
        TotalDebits = 30m,
        TotalCredits = 5m,
        Transactions =
        [
          new PdfTransactionDto { Description = "  Groceries  ", Details = "  Market  ", Date = "2026-10-01", SignedAmount = "-30.00", Currency = "EUR", Reference = "ref-1", PageNumber = 1 },
          new PdfTransactionDto { Description = "Refund", Date = "2026-10-02", SignedAmount = "5.00", Currency = "EUR", PageNumber = 1 }
        ]
      }
    }), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(2, saved!.Count);
    Assert.Equal("Groceries", saved[0].Name);
    Assert.Equal("Market; reference: ref-1", saved[0].Description);
    Assert.Equal(TransactionTypeEnum.Expense, saved[0].TransactionType);
    Assert.Equal(TransactionTypeEnum.Income, saved[1].TransactionType);
    Assert.Equal(30m, saved[0].Value.Amount);
    UnitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    _pdfImports.Verify(repository => repository.CreateAsync(
      It.Is<PdfImportRecord>(record => record.Id == Guid.Parse(correlationId) && record.UserId == _user.Id),
      It.IsAny<CancellationToken>()), Times.Once);
    _signalR.Verify(service => service.SendToClientGroupMethodAsync(_user.Email,
      HubConstants.TRANSACTIONS_IMPORTED_NOTIFICATION, correlationId), Times.Once);
    RabbitMqClientMock.Verify(client => client.PublishToQueueAsync(_settings.RequestQueue,
      It.Is<string>(message => message.Contains("Groceries") && message.Contains("Food")), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task CompletePdfImport_DuplicateResultDoesNotCreateTransactionsAgain()
  {
    var correlationId = Guid.NewGuid();
    _pdfImports.Setup(repository => repository.GetByIdAsync(correlationId, true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new PdfImportRecord(correlationId, _user.Id));
    UserRepositoryMock.Setup(repository => repository.GetByIdAsync(_user.Id, true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(_user);
    var handler = CreateCompleteHandler();

    var result = await handler.Handle(new CompletePdfImportCommand(new PdfImportResultDto
    {
      CorrelationId = correlationId.ToString(),
      UserId = _user.Id.ToString(),
      Success = true,
      Data = new PdfStatementDto
      {
        StatementCurrency = "EUR",
        Transactions = [new PdfTransactionDto { Description = "A", Date = "2026-10-01", SignedAmount = "-1.00", Currency = "EUR", PageNumber = 1 }]
      }
    }), CancellationToken.None);

    Assert.True(result.IsSuccess);
    TransactionRepositoryMock.Verify(repository => repository.BatchCreateTransactionsAsync(
      It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task CompletePdfImport_InvalidStatementReturnsFailureAndNotifiesClient()
  {
    var handler = CreateCompleteHandler();
    UserRepositoryMock.Setup(repository => repository.GetByIdAsync(_user.Id, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(_user);
    var correlationId = Guid.NewGuid().ToString();

    var result = await handler.Handle(new CompletePdfImportCommand(new PdfImportResultDto
    {
      CorrelationId = correlationId,
      UserId = _user.Id.ToString(),
      Success = true,
      Data = new PdfStatementDto
      {
        StatementCurrency = "EUR",
        Transactions = [new PdfTransactionDto { Description = "Missing fields" }]
      }
    }), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal("Parsing error.", result.ApplicationError!.Message);
    _signalR.Verify(service => service.SendToClientGroupMethodAsync(_user.Email,
      HubConstants.TRANSACTIONS_IMPORT_FAILED_NOTIFICATION,
      It.Is<string>(message => message.Contains(correlationId) && message.Contains("Transaction 1"))), Times.Once);
    _importStatuses.Verify(store => store.SetAsync(_user.Email, Guid.Parse(correlationId), "failed",
      It.Is<string?>(message => message != null && message.Contains("Transaction 1")), It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(repository => repository.BatchCreateTransactionsAsync(
      It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task CompletePdfImport_ProcessorFailureNotifiesClient()
  {
    var handler = CreateCompleteHandler();
    UserRepositoryMock.Setup(repository => repository.GetByIdAsync(_user.Id, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(_user);
    var correlationId = Guid.NewGuid().ToString();

    var result = await handler.Handle(new CompletePdfImportCommand(new PdfImportResultDto
    {
      CorrelationId = correlationId,
      UserId = _user.Id.ToString(),
      Success = false,
      Error = "Model rejected document"
    }), CancellationToken.None);

    Assert.False(result.IsSuccess);
    _signalR.Verify(service => service.SendToClientGroupMethodAsync(_user.Email,
      HubConstants.TRANSACTIONS_IMPORT_FAILED_NOTIFICATION,
      It.Is<string>(message => message.Contains("Model rejected document"))), Times.Once);
    _importStatuses.Verify(store => store.SetAsync(_user.Email, Guid.Parse(correlationId), "failed",
      "Model rejected document", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task CompletePdfImport_InvalidIdentifiersAreRejected()
  {
    var handler = CreateCompleteHandler();

    await Assert.ThrowsAsync<InvalidDataException>(() => handler.Handle(new CompletePdfImportCommand(new PdfImportResultDto
    {
      CorrelationId = "bad-correlation",
      UserId = "bad-user",
      Success = false
    }), CancellationToken.None));
  }

  private CompletePdfImportCommandHandler CreateCompleteHandler() => new(
    UserRepositoryMock.Object,
    _pdfImports.Object,
    TransactionRepositoryMock.Object,
    TransactionGroupRepositoryMock.Object,
    ExchangeRateServiceMock.Object,
    UnitOfWorkMock.Object,
    RabbitMqClientMock.Object,
    _signalR.Object,
    Options.Create(_settings),
    _importStatuses.Object,
    CreateLoggerMock<CompletePdfImportCommandHandler>().Object);

  private void RabbitMQClientMockSetup()
  {
    RabbitMqClientMock.Setup(client => client.PublishToQueueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
  }

  private static UploadPdfFileDto CreateUpload(string fileName, string content, string correlationId)
  {
    var bytes = System.Text.Encoding.ASCII.GetBytes(content);
    var stream = new MemoryStream(bytes);
    var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
    {
      Headers = new HeaderDictionary(),
      ContentType = "application/pdf"
    };
    return new UploadPdfFileDto { File = file, CorrelationId = correlationId };
  }
}
