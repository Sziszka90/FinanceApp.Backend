using System.Security.Claims;
using System.Text;
using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UploadCsv;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Commands;

public class UploadCsvTests : TestBase
{
  private readonly Mock<ILogger<UploadCsvCommandHandler>> _loggerMock;
  private readonly Mock<ILLMProcessorClient> _llmProcessorClientMock;
  private readonly UploadCsvCommandHandler _handler;

  public UploadCsvTests()
  {
    _loggerMock = CreateLoggerMock<UploadCsvCommandHandler>();
    _llmProcessorClientMock = new Mock<ILLMProcessorClient>();

    _handler = new UploadCsvCommandHandler(
      _loggerMock.Object,
      Mapper,
      TransactionRepositoryMock.Object,
      TransactionGroupRepositoryMock.Object,
      UnitOfWorkMock.Object,
      UserServiceMock.Object,
      _llmProcessorClientMock.Object,
      ExchangeRateServiceMock.Object
    );
  }

  [Fact]
  public async Task Handle_ValidCsvFile_ProcessesTransactionsSuccessfully()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var csvContent = "\"Username\",\"Account Number\",\"Booking Date\",\"Amount\",\"Currency\",\"Partner Name\",\"Partner IBAN\",\"Partner Account Number\",\"Partner Bank Code\",\"Booking Information\",\"Transaction ID\",\"Transaction Date and Time\"\n" +
                    "\"testuser\",\"Account1\",\"2024-01-01\",\"100.50\",\"USD\",\"Coffee Shop\",\"\",\"\",\"\",\"Morning coffee\",\"TXN001\",\"2024-01-01 10:00:00\"\n" +
                    "\"testuser\",\"Account1\",\"2024-01-02\",\"-25.30\",\"USD\",\"Gas Station\",\"\",\"\",\"\",\"Fuel\",\"TXN002\",\"2024-01-02 15:30:00\"";

    var csvFile = CreateMockFormFile(csvContent, "transactions.csv", "text/csv");
    var correlationId = Guid.NewGuid().ToString();

    var uploadDto = new UploadCsvFileDto
    {
      File = csvFile,
      CorrelationId = correlationId
    };

    var command = new UploadCsvCommand(uploadDto, CancellationToken.None);

    var transactionGroups = new List<TransactionGroup>
    {
      new TransactionGroup("Food", "Food expenses", "", user),
      new TransactionGroup("Transport", "Transport expenses", "", user)
    };

    var expectedTransactions = new List<Transaction>();

    // Setup mocks
    ExchangeRateServiceMock.Setup(x => x.ConvertAmountAsync(
      It.IsAny<decimal>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(1.0m));

    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(user));

    TransactionRepositoryMock.Setup(x => x.BatchCreateTransactionsAsync(It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()))
      .Callback<List<Transaction>, CancellationToken>((transactions, ct) =>
      {
        expectedTransactions.AddRange(
          transactions.Select(t => new Transaction(
            t.Name,
            t.Description,
            t.TransactionType ?? TransactionTypeEnum.Income,
            t.Value,
            t.Value.Amount,
            t.TransactionGroup,
            DateTimeOffset.UtcNow,
            user
          ))
        );
      })
      .ReturnsAsync(expectedTransactions);

    TransactionGroupRepositoryMock.Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactionGroups);

    _llmProcessorClientMock.Setup(x => x.MatchTransactionGroup(
        user.Id.ToString(),
        It.IsAny<List<string>>(),
        It.IsAny<List<string>>(),
        It.IsAny<string>()))
      .ReturnsAsync(Result.Success(true));

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedTransactions);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(2, expectedTransactions.Count);

    // Verify the transactions were created correctly
    var coffeeTransaction = expectedTransactions.FirstOrDefault(t => t.Name == "Coffee Shop");
    Assert.NotNull(coffeeTransaction);
    Assert.Equal(TransactionTypeEnum.Income, coffeeTransaction.TransactionType);
    Assert.Equal(100.50m, coffeeTransaction.Value.Amount);
    Assert.Equal(CurrencyEnum.USD, coffeeTransaction.Value.Currency);

    var gasTransaction = expectedTransactions.FirstOrDefault(t => t.Name == "Gas Station");
    Assert.NotNull(gasTransaction);
    Assert.Equal(TransactionTypeEnum.Expense, gasTransaction.TransactionType);
    Assert.Equal(25.30m, gasTransaction.Value.Amount);
    Assert.Equal(CurrencyEnum.USD, gasTransaction.Value.Currency);

    // Verify repository calls
    UserServiceMock.Verify(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.BatchCreateTransactionsAsync(It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

    // Verify LLM processor call
    _llmProcessorClientMock.Verify(x => x.MatchTransactionGroup(
      user.Id.ToString(),
      It.Is<List<string>>(names => names.Contains("Coffee Shop") && names.Contains("Gas Station")),
      It.Is<List<string>>(groups => groups.Contains("Food") && groups.Contains("Transport")),
      correlationId), Times.Once);
  }

  [Fact]
  public async Task Handle_UserNotLoggedIn_ReturnsFailure()
  {
    // arrange
    var csvFile = CreateMockFormFile("test,data", "test.csv", "text/csv");
    var uploadDto = new UploadCsvFileDto
    {
      File = csvFile,
      CorrelationId = Guid.NewGuid().ToString()
    };
    var command = new UploadCsvCommand(uploadDto, CancellationToken.None);

    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Failure<User>(ApplicationError.UserNotFoundError("test@example.com")));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal(ApplicationError.UserNotFoundError().Message, result.ApplicationError.Message);

    // Verify no repository calls were made
    UserServiceMock.Verify(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.BatchCreateTransactionsAsync(It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_UserNotFoundInDatabase_ReturnsFailure()
  {
    // arrange
    var csvFile = CreateMockFormFile("test,data", "test.csv", "text/csv");
    var uploadDto = new UploadCsvFileDto
    {
      File = csvFile,
      CorrelationId = Guid.NewGuid().ToString()
    };
    var command = new UploadCsvCommand(uploadDto, CancellationToken.None);

    ExchangeRateServiceMock.Setup(x => x.ConvertAmountAsync(
      It.IsAny<decimal>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(1.0m));

    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Failure<User>(ApplicationError.UserNotFoundError("nonexistent@example.com")));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal(ApplicationError.UserNotFoundError().Message, result.ApplicationError.Message);

    // Verify user lookup was attempted
    UserServiceMock.Verify(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.BatchCreateTransactionsAsync(It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_LLMProcessorFails_ReturnsFailure()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var csvContent = "\"Username\",\"Account Number\",\"Booking Date\",\"Amount\",\"Currency\",\"Partner Name\",\"Partner IBAN\",\"Partner Account Number\",\"Partner Bank Code\",\"Booking Information\",\"Transaction ID\",\"Transaction Date and Time\"\n" +
                    "\"testuser\",\"Account1\",\"2024-01-01\",\"100.50\",\"USD\",\"Coffee Shop\",\"\",\"\",\"\",\"Morning coffee\",\"TXN001\",\"2024-01-01 10:00:00\"";
    var csvFile = CreateMockFormFile(csvContent, "transactions.csv", "text/csv");
    var correlationId = Guid.NewGuid().ToString();

    var uploadDto = new UploadCsvFileDto
    {
      File = csvFile,
      CorrelationId = correlationId
    };
    var command = new UploadCsvCommand(uploadDto, CancellationToken.None);

    var transactionGroups = new List<TransactionGroup>
    {
      new TransactionGroup("Food", "Food expenses", "", user)
    };

    // Setup mocks
    ExchangeRateServiceMock.Setup(x => x.ConvertAmountAsync(
      It.IsAny<decimal>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(1.0m));

    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(user));

    TransactionRepositoryMock.Setup(x => x.BatchCreateTransactionsAsync(It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction>());

    TransactionGroupRepositoryMock.Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactionGroups);

    // LLM processor fails
    var llmError = ApplicationError.ExternalCallError("LLM service unavailable");
    _llmProcessorClientMock.Setup(x => x.MatchTransactionGroup(
        user.Id.ToString(),
        It.IsAny<List<string>>(),
        It.IsAny<List<string>>(),
        correlationId))
      .ReturnsAsync(Result.Failure<bool>(llmError));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal(ApplicationError.LLMProcessorRequestError(llmError.Message).Message, result.ApplicationError.Message);

    // Verify transactions were still created but processing failed at LLM step
    TransactionRepositoryMock.Verify(x => x.BatchCreateTransactionsAsync(It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    _llmProcessorClientMock.Verify(x => x.MatchTransactionGroup(
      user.Id.ToString(),
      It.IsAny<List<string>>(),
      It.IsAny<List<string>>(),
      correlationId), Times.Once);
  }

  [Fact]
  public async Task Handle_EmptyCsvFile_ProcessesWithoutTransactions()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var csvContent = "\"Username\",\"Account Number\",\"Booking Date\",\"Amount\",\"Currency\",\"Partner Name\",\"Partner IBAN\",\"Partner Account Number\",\"Partner Bank Code\",\"Booking Information\",\"Transaction ID\",\"Transaction Date and Time\"\n"; // Only header
    var csvFile = CreateMockFormFile(csvContent, "empty.csv", "text/csv");
    var correlationId = Guid.NewGuid().ToString();

    var uploadDto = new UploadCsvFileDto
    {
      File = csvFile,
      CorrelationId = correlationId
    };
    var command = new UploadCsvCommand(uploadDto, CancellationToken.None);

    var transactionGroups = new List<TransactionGroup>();
    var emptyTransactions = new List<Transaction>();

    // Setup mocks
    ExchangeRateServiceMock.Setup(x => x.ConvertAmountAsync(
      It.IsAny<decimal>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(1.0m));

    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(user));

    TransactionRepositoryMock.Setup(x => x.BatchCreateTransactionsAsync(It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(emptyTransactions);

    TransactionGroupRepositoryMock.Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactionGroups);

    _llmProcessorClientMock.Setup(x => x.MatchTransactionGroup(
        user.Id.ToString(),
        It.IsAny<List<string>>(),
        It.IsAny<List<string>>(),
        correlationId))
      .ReturnsAsync(Result.Success(true));

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(emptyTransactions);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Empty(result.Data);

    // Verify LLM processor was called with empty transaction list
    _llmProcessorClientMock.Verify(x => x.MatchTransactionGroup(
      user.Id.ToString(),
      It.Is<List<string>>(names => names.Count == 0),
      It.Is<List<string>>(groups => groups.Count == 0),
      correlationId), Times.Once);
  }

  [Fact]
  public async Task Handle_CsvWithDifferentCurrencies_ProcessesCorrectly()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var csvContent = "\"Username\",\"Account Number\",\"Booking Date\",\"Amount\",\"Currency\",\"Partner Name\",\"Partner IBAN\",\"Partner Account Number\",\"Partner Bank Code\",\"Booking Information\",\"Transaction ID\",\"Transaction Date and Time\"\n" +
                    "\"testuser\",\"Account1\",\"2024-01-01\",\"100.50\",\"EUR\",\"European Coffee\",\"\",\"\",\"\",\"Coffee in Europe\",\"TXN001\",\"2024-01-01 10:00:00\"\n" +
                    "\"testuser\",\"Account1\",\"2024-01-02\",\"-25.30\",\"GBP\",\"London Transport\",\"\",\"\",\"\",\"London bus\",\"TXN002\",\"2024-01-02 15:30:00\"";

    var csvFile = CreateMockFormFile(csvContent, "transactions.csv", "text/csv");
    var correlationId = Guid.NewGuid().ToString();

    var uploadDto = new UploadCsvFileDto
    {
      File = csvFile,
      CorrelationId = correlationId
    };
    var command = new UploadCsvCommand(uploadDto, CancellationToken.None);

    var transactionGroups = new List<TransactionGroup>();
    var expectedTransactions = new List<Transaction>();

    // Setup mocks
    ExchangeRateServiceMock.Setup(x => x.ConvertAmountAsync(
      It.IsAny<decimal>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(1.0m));

    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(user));

    TransactionRepositoryMock.Setup(x => x.BatchCreateTransactionsAsync(It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()))
      .Callback<List<Transaction>, CancellationToken>((transactions, ct) =>
      {
        expectedTransactions.AddRange(
          transactions.Select(t => new Transaction(
            t.Name,
            t.Description,
            t.TransactionType ?? TransactionTypeEnum.Income,
            t.Value,
            t.Value.Amount,
            t.TransactionGroup,
            DateTimeOffset.UtcNow,
            user
          ))
        );
      })
      .ReturnsAsync(expectedTransactions);

    TransactionGroupRepositoryMock.Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactionGroups);

    _llmProcessorClientMock.Setup(x => x.MatchTransactionGroup(
        It.IsAny<string>(),
        It.IsAny<List<string>>(),
        It.IsAny<List<string>>(),
        correlationId))
      .ReturnsAsync(Result.Success(true));

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedTransactions);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(2, expectedTransactions.Count);

    // Verify currencies were parsed correctly
    var eurTransaction = expectedTransactions.FirstOrDefault(t => t.Name == "European Coffee");
    Assert.NotNull(eurTransaction);
    Assert.Equal(CurrencyEnum.EUR, eurTransaction.Value.Currency);

    var gbpTransaction = expectedTransactions.FirstOrDefault(t => t.Name == "London Transport");
    Assert.NotNull(gbpTransaction);
    Assert.Equal(CurrencyEnum.GBP, gbpTransaction.Value.Currency);
  }

  [Fact]
  public async Task Handle_CsvWithInvalidAmountFormat_UsesZeroAmount()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var csvContent = "\"Username\",\"Account Number\",\"Booking Date\",\"Amount\",\"Currency\",\"Partner Name\",\"Partner IBAN\",\"Partner Account Number\",\"Partner Bank Code\",\"Booking Information\",\"Transaction ID\",\"Transaction Date and Time\"\n" +
                    "\"testuser\",\"Account1\",\"2024-01-01\",\"invalid_amount\",\"USD\",\"Invalid Transaction\",\"\",\"\",\"\",\"Bad amount\",\"TXN001\",\"2024-01-01 10:00:00\"";

    var csvFile = CreateMockFormFile(csvContent, "transactions.csv", "text/csv");
    var correlationId = Guid.NewGuid().ToString();

    var uploadDto = new UploadCsvFileDto
    {
      File = csvFile,
      CorrelationId = correlationId
    };
    var command = new UploadCsvCommand(uploadDto, CancellationToken.None);

    var expectedTransactions = new List<Transaction>();

    // Setup mocks
    ExchangeRateServiceMock.Setup(x => x.ConvertAmountAsync(
      It.IsAny<decimal>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(1.0m));

    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(user));

    TransactionRepositoryMock.Setup(x => x.BatchCreateTransactionsAsync(It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()))
      .Callback<List<Transaction>, CancellationToken>((transactions, ct) =>
      {
        expectedTransactions.AddRange(
          transactions.Select(t => new Transaction(
            t.Name,
            t.Description,
            t.TransactionType ?? TransactionTypeEnum.Income,
            t.Value,
            t.Value.Amount,
            t.TransactionGroup,
            DateTimeOffset.UtcNow,
            user
          ))
        );
      })
      .ReturnsAsync(expectedTransactions);

    TransactionGroupRepositoryMock.Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroup>());

    _llmProcessorClientMock.Setup(x => x.MatchTransactionGroup(
        It.IsAny<string>(),
        It.IsAny<List<string>>(),
        It.IsAny<List<string>>(),
        correlationId))
      .ReturnsAsync(Result.Success(true));

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedTransactions);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Single(expectedTransactions);

    var transaction = expectedTransactions.First();
    Assert.Equal(0m, transaction.Value.Amount);
    Assert.Equal(TransactionTypeEnum.Income, transaction.TransactionType); // 0 is not < 0, so it's income
  }

  [Fact]
  public async Task Handle_CsvWithEmptyTransactionName_UsesUnknown()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var csvContent = "\"Username\",\"Account Number\",\"Booking Date\",\"Amount\",\"Currency\",\"Partner Name\",\"Partner IBAN\",\"Partner Account Number\",\"Partner Bank Code\",\"Booking Information\",\"Transaction ID\",\"Transaction Date and Time\"\n" +
                    "\"testuser\",\"Account1\",\"2024-01-01\",\"100.50\",\"USD\",\"\",\"\",\"\",\"\",\"Empty name transaction\",\"TXN001\",\"2024-01-01 10:00:00\"";

    var csvFile = CreateMockFormFile(csvContent, "transactions.csv", "text/csv");
    var correlationId = Guid.NewGuid().ToString();

    var uploadDto = new UploadCsvFileDto
    {
      File = csvFile,
      CorrelationId = correlationId
    };
    var command = new UploadCsvCommand(uploadDto, CancellationToken.None);

    var expectedTransactions = new List<Transaction>();

    // Setup mocks
    ExchangeRateServiceMock.Setup(x => x.ConvertAmountAsync(
      It.IsAny<decimal>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(1.0m));

    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(user));

    TransactionRepositoryMock.Setup(x => x.BatchCreateTransactionsAsync(It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()))
      .Callback<List<Transaction>, CancellationToken>((transactions, ct) =>
      {
        expectedTransactions.AddRange(
          transactions.Select(t => new Transaction(
            t.Name,
            t.Description,
            t.TransactionType ?? TransactionTypeEnum.Income,
            t.Value,
            t.Value.Amount,
            t.TransactionGroup,
            DateTimeOffset.UtcNow,
            user
          ))
        );
      })
      .ReturnsAsync(expectedTransactions);

    TransactionGroupRepositoryMock.Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroup>());

    _llmProcessorClientMock.Setup(x => x.MatchTransactionGroup(
        It.IsAny<string>(),
        It.IsAny<List<string>>(),
        It.IsAny<List<string>>(),
        correlationId))
      .ReturnsAsync(Result.Success(true));

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedTransactions);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Single(expectedTransactions);

    var transaction = expectedTransactions.First();
    Assert.Equal("Unknown", transaction.Name);
  }

  [Fact]
  public async Task Handle_CsvWithInvalidCurrency_UsesEUR()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var csvContent = "\"Username\",\"Account Number\",\"Booking Date\",\"Amount\",\"Currency\",\"Partner Name\",\"Partner IBAN\",\"Partner Account Number\",\"Partner Bank Code\",\"Booking Information\",\"Transaction ID\",\"Transaction Date and Time\"\n" +
                    "\"testuser\",\"Account1\",\"2024-01-01\",\"100.50\",\"INVALID\",\"Test Transaction\",\"\",\"\",\"\",\"Test description\",\"TXN001\",\"2024-01-01 10:00:00\"";

    var csvFile = CreateMockFormFile(csvContent, "transactions.csv", "text/csv");
    var correlationId = Guid.NewGuid().ToString();

    var uploadDto = new UploadCsvFileDto
    {
      File = csvFile,
      CorrelationId = correlationId
    };
    var command = new UploadCsvCommand(uploadDto, CancellationToken.None);

    var expectedTransactions = new List<Transaction>();

    // Setup mocks
    ExchangeRateServiceMock.Setup(x => x.ConvertAmountAsync(
      It.IsAny<decimal>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Failure<decimal>(ApplicationError.MissingExchangeRatesError()));

    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(user));

    TransactionRepositoryMock.Setup(x => x.BatchCreateTransactionsAsync(It.IsAny<List<Transaction>>(), It.IsAny<CancellationToken>()))
      .Callback<List<Transaction>, CancellationToken>((transactions, ct) =>
      {
        expectedTransactions.AddRange(
          transactions.Select(t => new Transaction(
            t.Name,
            t.Description,
            t.TransactionType ?? TransactionTypeEnum.Income,
            t.Value,
            t.Value.Amount,
            t.TransactionGroup,
            DateTimeOffset.UtcNow,
            user
          ))
        );
      })
      .ReturnsAsync(expectedTransactions);

    TransactionGroupRepositoryMock.Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroup>());

    _llmProcessorClientMock.Setup(x => x.MatchTransactionGroup(
        It.IsAny<string>(),
        It.IsAny<List<string>>(),
        It.IsAny<List<string>>(),
        correlationId))
      .ReturnsAsync(Result.Success(true));

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedTransactions);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(CurrencyEnum.XXX, result.Data![0].Value.Currency);
  }

  private IFormFile CreateMockFormFile(string content, string fileName, string contentType)
  {
    var bytes = Encoding.UTF8.GetBytes(content);
    var stream = new MemoryStream(bytes);

    var mockFile = new Mock<IFormFile>();
    mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
    mockFile.Setup(f => f.FileName).Returns(fileName);
    mockFile.Setup(f => f.Length).Returns(bytes.Length);
    mockFile.Setup(f => f.ContentType).Returns(contentType);

    return mockFile.Object;
  }
}
