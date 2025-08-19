using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.UserApi.UserCommands.CreateUser;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.UserTests.Commands;

public class CreateUserTests : TestBase
{
  private readonly Mock<ILogger<CreateUserCommandHandler>> _loggerMock;
  private readonly CreateUserCommandHandler _handler;

  public CreateUserTests()
  {
    _loggerMock = CreateLoggerMock<CreateUserCommandHandler>();

    _handler = new CreateUserCommandHandler(
      _loggerMock.Object,
      Mapper,
      UserRepositoryMock.Object,
      TransactionGroupRepositoryMock.Object,
      UnitOfWorkMock.Object,
      SmtpEmailSenderMock.Object,
      BcryptServiceMock.Object,
      TokenServiceMock.Object
    );
  }

  [Fact]
  public async Task CreateUserHandler_ValidRequest_ReturnsSuccessResult()
  {
    // arrange
    var createUserDto = new CreateUserDto
    {
      UserName = "testuser",
      Email = "test@example.com",
      Password = "TestPassword123!",
      BaseCurrency = CurrencyEnum.USD
    };

    var command = new CreateUserCommand(createUserDto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(createUserDto.UserName, result.Data.UserName);
    Assert.Equal(createUserDto.Email, result.Data.Email);

    UserRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u =>
      u.UserName == createUserDto.UserName &&
      u.Email == createUserDto.Email), It.IsAny<CancellationToken>()), Times.Once);

    TransactionGroupRepositoryMock.Verify(x => x.BatchCreateTransactionGroupsAsync(
      It.Is<List<TransactionGroup>>(groups => groups.Count > 0), It.IsAny<CancellationToken>()), Times.Once);

    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(createUserDto.Email, TokenType.EmailConfirmation), Times.Once);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(It.IsAny<User>(), "default_token"), Times.Once);
  }

  [Fact]
  public async Task CreateUserHandler_UserNameAlreadyExists_ReturnsFailureResult()
  {
    // arrange
    var createUserDto = new CreateUserDto
    {
      UserName = "existinguser",
      Email = "test@example.com",
      Password = "TestPassword123!",
      BaseCurrency = CurrencyEnum.USD
    };

    var command = new CreateUserCommand(createUserDto, CancellationToken.None);
    var existingUser = new User("existinguser", "other@example.com", "hash", CurrencyEnum.EUR);

    UserRepositoryMock
      .SetupSequence(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<User> { existingUser })
      .ReturnsAsync(new List<User>());

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USERNAME_ALREADY_EXISTS", result.ApplicationError.Code);

    UserRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task CreateUserHandler_EmailAlreadyExists_ReturnsFailureResult()
  {
    // arrange
    var createUserDto = new CreateUserDto
    {
      UserName = "testuser",
      Email = "existing@example.com",
      Password = "TestPassword123!",
      BaseCurrency = CurrencyEnum.USD
    };

    var command = new CreateUserCommand(createUserDto, CancellationToken.None);
    var existingUser = new User("otheruser", "existing@example.com", "hash", CurrencyEnum.EUR);

    UserRepositoryMock
      .SetupSequence(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<User>())
      .ReturnsAsync(new List<User> { existingUser });

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USEREMAIL_ALREADY_EXISTS", result.ApplicationError.Code);

    UserRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task CreateUserHandler_TokenGenerationFails_ReturnsFailureResult()
  {
    // arrange
    var createUserDto = new CreateUserDto
    {
      UserName = "testuser",
      Email = "test@example.com",
      Password = "TestPassword123!",
      BaseCurrency = CurrencyEnum.USD
    };

    var command = new CreateUserCommand(createUserDto, CancellationToken.None);
    var tokenError = ApplicationError.TokenGenerationError();

    TokenServiceMock
      .Setup(x => x.GenerateTokenAsync(createUserDto.Email, TokenType.EmailConfirmation))
      .ReturnsAsync(Result.Failure<string>(tokenError));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("TOKEN_GENERATION_ERROR", result.ApplicationError.Code);

    UserRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    TransactionGroupRepositoryMock.Verify(x => x.BatchCreateTransactionGroupsAsync(
      It.IsAny<List<TransactionGroup>>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task CreateUserHandler_EmailSendingFails_ReturnsFailureResult()
  {
    // arrange
    var createUserDto = new CreateUserDto
    {
      UserName = "testuser",
      Email = "test@example.com",
      Password = "TestPassword123!",
      BaseCurrency = CurrencyEnum.USD
    };

    var command = new CreateUserCommand(createUserDto, CancellationToken.None);
    var emailError = ApplicationError.EmailConfirmationError(createUserDto.Email);

    SmtpEmailSenderMock
      .Setup(x => x.SendEmailConfirmationAsync(It.IsAny<User>(), "default_token"))
      .ReturnsAsync(Result.Failure<bool>(emailError));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USEREMAIL_CONFIRMATION_ERROR", result.ApplicationError.Code);

    UserRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    TransactionGroupRepositoryMock.Verify(x => x.BatchCreateTransactionGroupsAsync(
      It.IsAny<List<TransactionGroup>>(), It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(createUserDto.Email, TokenType.EmailConfirmation), Times.Once);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(It.IsAny<User>(), "default_token"), Times.Once);
  }

  [Theory]
  [InlineData(CurrencyEnum.USD)]
  [InlineData(CurrencyEnum.EUR)]
  [InlineData(CurrencyEnum.GBP)]
  public async Task CreateUserHandler_DifferentBaseCurrencies_CreatesUserWithCorrectCurrency(CurrencyEnum baseCurrency)
  {
    // arrange
    var createUserDto = new CreateUserDto
    {
      UserName = "testuser",
      Email = "test@example.com",
      Password = "TestPassword123!",
      BaseCurrency = baseCurrency
    };

    var command = new CreateUserCommand(createUserDto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(baseCurrency, result.Data.BaseCurrency);

    UserRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u => u.BaseCurrency == baseCurrency), It.IsAny<CancellationToken>()), Times.Once);
  }
}
