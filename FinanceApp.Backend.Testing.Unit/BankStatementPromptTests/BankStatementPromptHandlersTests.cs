using System.Linq.Expressions;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.BankStatementPromptApi.BankStatementPromptCommands;
using FinanceApp.Backend.Application.BankStatementPromptApi.BankStatementPromptQueries;
using FinanceApp.Backend.Application.Dtos.BankStatementPromptDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Validators;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.BankStatementPromptTests;

public class BankStatementPromptHandlersTests : TestBase
{
  private readonly Mock<IRepository<BankStatementPrompt>> _prompts = new();
  private readonly User _user = new(null, "test-user", "test@example.com", true, "hash", CurrencyEnum.EUR);

  public BankStatementPromptHandlersTests()
  {
    UserServiceMock.Setup(service => service.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result<User>.Success(_user));
  }

  [Fact]
  public async Task SavePrompt_CreatesTrimmedPromptAndReturnsDto()
  {
    var handler = new SaveBankStatementPromptCommandHandler(_prompts.Object, UserServiceMock.Object, UnitOfWorkMock.Object);
    var dto = new SaveBankStatementPromptDto { Name = "  My Bank  ", Instructions = "  Extract rows  " };
    _prompts.Setup(repository => repository.CreateAsync(It.IsAny<BankStatementPrompt>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((BankStatementPrompt prompt, CancellationToken _) => prompt);

    var result = await handler.Handle(new SaveBankStatementPromptCommand(dto), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("My Bank", result.Data!.Name);
    Assert.Equal("Extract rows", result.Data.Instructions);
    Assert.False(result.Data.IsSelected);
    _prompts.Verify(repository => repository.CreateAsync(It.Is<BankStatementPrompt>(prompt =>
      prompt.UserId == _user.Id && prompt.User == _user && prompt.Name == "My Bank" && prompt.Instructions == "Extract rows"),
      It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SavePrompt_UpdatesOwnedPrompt()
  {
    var prompt = new BankStatementPrompt { UserId = _user.Id, Name = "Old", Instructions = "Old prompt", IsSelected = true };
    var handler = new SaveBankStatementPromptCommandHandler(_prompts.Object, UserServiceMock.Object, UnitOfWorkMock.Object);
    _prompts.Setup(repository => repository.GetSingleOrDefaultAsync(It.IsAny<Expression<Func<BankStatementPrompt, bool>>>(),
        false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(prompt);

    var result = await handler.Handle(
      new SaveBankStatementPromptCommand(new SaveBankStatementPromptDto { Name = "  New  ", Instructions = "  New instructions  " }, prompt.Id),
      CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("New", prompt.Name);
    Assert.Equal("New instructions", prompt.Instructions);
    Assert.True(result.Data!.IsSelected);
    UnitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SavePrompt_ReturnsNotFoundForUnownedPrompt()
  {
    var handler = new SaveBankStatementPromptCommandHandler(_prompts.Object, UserServiceMock.Object, UnitOfWorkMock.Object);
    _prompts.Setup(repository => repository.GetSingleOrDefaultAsync(It.IsAny<Expression<Func<BankStatementPrompt, bool>>>(),
        false, It.IsAny<CancellationToken>()))
      .ReturnsAsync((BankStatementPrompt?)null);

    var result = await handler.Handle(
      new SaveBankStatementPromptCommand(new SaveBankStatementPromptDto { Name = "Name", Instructions = "Prompt" }, Guid.NewGuid()),
      CancellationToken.None);

    Assert.False(result.IsSuccess);
    _prompts.Verify(repository => repository.CreateAsync(It.IsAny<BankStatementPrompt>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task DeletePrompt_DeletesOwnedPrompt()
  {
    var prompt = new BankStatementPrompt { UserId = _user.Id, Name = "My Bank", Instructions = "Prompt" };
    var handler = new DeleteBankStatementPromptCommandHandler(_prompts.Object, UserServiceMock.Object, UnitOfWorkMock.Object);
    _prompts.Setup(repository => repository.GetSingleOrDefaultAsync(It.IsAny<Expression<Func<BankStatementPrompt, bool>>>(),
        false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(prompt);

    var result = await handler.Handle(new DeleteBankStatementPromptCommand(prompt.Id), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.True(result.Data);
    _prompts.Verify(repository => repository.Delete(prompt), Times.Once);
    UnitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task DeletePrompt_ReturnsNotFoundWhenPromptIsNotOwned()
  {
    var handler = new DeleteBankStatementPromptCommandHandler(_prompts.Object, UserServiceMock.Object, UnitOfWorkMock.Object);
    _prompts.Setup(repository => repository.GetSingleOrDefaultAsync(It.IsAny<Expression<Func<BankStatementPrompt, bool>>>(),
        false, It.IsAny<CancellationToken>()))
      .ReturnsAsync((BankStatementPrompt?)null);

    var result = await handler.Handle(new DeleteBankStatementPromptCommand(Guid.NewGuid()), CancellationToken.None);

    Assert.False(result.IsSuccess);
    _prompts.Verify(repository => repository.Delete(It.IsAny<BankStatementPrompt>()), Times.Never);
    UnitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task SelectPrompt_UpdatesSelectionForAllOwnedPrompts()
  {
    var selected = new BankStatementPrompt { UserId = _user.Id, Name = "Selected", Instructions = "A", IsSelected = false };
    var previous = new BankStatementPrompt { UserId = _user.Id, Name = "Previous", Instructions = "B", IsSelected = true };
    var prompts = new List<BankStatementPrompt> { selected, previous };
    var handler = new SelectBankStatementPromptCommandHandler(_prompts.Object, UserServiceMock.Object, UnitOfWorkMock.Object);
    _prompts.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<BankStatementPrompt, bool>>>(),
        false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(prompts);

    var result = await handler.Handle(new SelectBankStatementPromptCommand(selected.Id), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.True(selected.IsSelected);
    Assert.False(previous.IsSelected);
    UnitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task SelectPrompt_ClearsSelectionWhenNullIsPassed()
  {
    var prompt = new BankStatementPrompt { UserId = _user.Id, Name = "Selected", Instructions = "A", IsSelected = true };
    var handler = new SelectBankStatementPromptCommandHandler(_prompts.Object, UserServiceMock.Object, UnitOfWorkMock.Object);
    _prompts.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<BankStatementPrompt, bool>>>(),
        false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<BankStatementPrompt> { prompt });

    var result = await handler.Handle(new SelectBankStatementPromptCommand(null), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.False(prompt.IsSelected);
  }

  [Fact]
  public async Task SelectPrompt_RejectsPromptOutsideCurrentUsersCollection()
  {
    var handler = new SelectBankStatementPromptCommandHandler(_prompts.Object, UserServiceMock.Object, UnitOfWorkMock.Object);
    _prompts.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<BankStatementPrompt, bool>>>(),
        false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<BankStatementPrompt>());

    var result = await handler.Handle(new SelectBankStatementPromptCommand(Guid.NewGuid()), CancellationToken.None);

    Assert.False(result.IsSuccess);
    UnitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task GetPrompts_ReturnsCurrentUsersPromptsSortedByName()
  {
    var prompts = new List<BankStatementPrompt>
    {
      new() { UserId = _user.Id, Name = "Zebra", Instructions = "Last", IsSelected = true },
      new() { UserId = _user.Id, Name = "Alpha", Instructions = "First" }
    };
    var handler = new GetBankStatementPromptsQueryHandler(_prompts.Object, UserServiceMock.Object);
    _prompts.Setup(repository => repository.GetAllAsync(It.IsAny<Expression<Func<BankStatementPrompt, bool>>>(),
        true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(prompts);

    var result = await handler.Handle(new GetBankStatementPromptsQuery(), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Collection(result.Data!,
      first => Assert.Equal("Alpha", first.Name),
      second =>
      {
        Assert.Equal("Zebra", second.Name);
        Assert.True(second.IsSelected);
        Assert.Equal("Last", second.Instructions);
      });
  }

  [Fact]
  public async Task GetPrompts_ReturnsFailureWhenThereIsNoActiveUser()
  {
    UserServiceMock.Setup(service => service.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Failure<User>(ApplicationError.UserNotFoundError()));
    var handler = new GetBankStatementPromptsQueryHandler(_prompts.Object, UserServiceMock.Object);

    var result = await handler.Handle(new GetBankStatementPromptsQuery(), CancellationToken.None);

    Assert.False(result.IsSuccess);
    _prompts.Verify(repository => repository.GetAllAsync(It.IsAny<Expression<Func<BankStatementPrompt, bool>>>(),
      true, It.IsAny<CancellationToken>()), Times.Never);
  }

  [Theory]
  [InlineData("", "Instructions", false)]
  [InlineData("A", "", false)]
  [InlineData("Valid bank", "Valid instructions", true)]
  public void SavePromptValidator_ValidatesRequiredFields(string name, string instructions, bool expectedValid)
  {
    var validator = new SaveBankStatementPromptDtoValidator();

    var result = validator.Validate(new SaveBankStatementPromptDto { Name = name, Instructions = instructions });

    Assert.Equal(expectedValid, result.IsValid);
  }

  [Fact]
  public void SavePromptCommandValidator_ValidatesNestedPromptDto()
  {
    var promptValidator = new SaveBankStatementPromptDtoValidator();
    var validator = new SaveBankStatementPromptCommandValidator(promptValidator);

    Assert.True(validator.Validate(new SaveBankStatementPromptCommand(
      new SaveBankStatementPromptDto { Name = "My bank", Instructions = "Extract transactions" })).IsValid);
    Assert.False(validator.Validate(new SaveBankStatementPromptCommand(new SaveBankStatementPromptDto())).IsValid);
  }

  [Fact]
  public void BankStatementPrompt_UpdateChangesNameAndInstructions()
  {
    var prompt = new BankStatementPrompt { Name = "Before", Instructions = "Old instructions" };

    prompt.Update("After", "New instructions");

    Assert.Equal("After", prompt.Name);
    Assert.Equal("New instructions", prompt.Instructions);
  }
}
