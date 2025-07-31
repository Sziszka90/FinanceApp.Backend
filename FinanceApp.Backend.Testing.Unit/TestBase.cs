using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit;

public abstract class TestBase
{
  protected readonly Mock<IUserRepository> UserRepositoryMock;
  protected readonly Mock<IUserRepository> UserRepositorySpecificMock;
  protected readonly Mock<ITransactionGroupRepository> TransactionGroupRepositoryMock;
  protected readonly Mock<ITransactionRepository> TransactionRepositoryMock;
  protected readonly Mock<IUnitOfWork> UnitOfWorkMock;
  protected readonly Mock<ISmtpEmailSender> SmtpEmailSenderMock;
  protected readonly Mock<IBcryptService> BcryptServiceMock;
  protected readonly Mock<ITokenService> TokenServiceMock;
  protected readonly Mock<IHttpContextAccessor> HttpContextAccessorMock;
  protected readonly IMapper Mapper;

  protected TestBase()
  {
    UserRepositoryMock = new Mock<IUserRepository>();
    UserRepositorySpecificMock = new Mock<IUserRepository>();
    TransactionGroupRepositoryMock = new Mock<ITransactionGroupRepository>();
    TransactionRepositoryMock = new Mock<ITransactionRepository>();
    UnitOfWorkMock = new Mock<IUnitOfWork>();
    SmtpEmailSenderMock = new Mock<ISmtpEmailSender>();
    BcryptServiceMock = new Mock<IBcryptService>();
    TokenServiceMock = new Mock<ITokenService>();
    HttpContextAccessorMock = new Mock<IHttpContextAccessor>();

    Mapper = new MapperConfiguration(cfg =>
    {
      cfg.CreateMap<User, GetUserDto>();
    }).CreateMapper();

    SetupBcryptServiceMock();
    SetupTokenServiceMock();
    SetupHttpContextAccessorMock();
    SetupUserRepositoryMock();
    SetupTransactionGroupRepositoryMock();
    SetupTransactionRepositoryMock();
    SetupUnitOfWorkMock();
    SetupSmtpEmailSenderMock();
  }

  protected virtual void SetupUserRepositoryMock()
  {
    UserRepositoryMock
      .Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<User>());

    UserRepositoryMock
      .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((User user, CancellationToken ct) => user);

    UserRepositoryMock
      .Setup(x => x.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((User?)null);

    UserRepositoryMock
      .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((User?)null);
  }

  protected virtual void SetupTransactionGroupRepositoryMock()
  {
    TransactionGroupRepositoryMock
      .Setup(x => x.BatchCreateTransactionGroupsAsync(It.IsAny<List<TransactionGroup>>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((List<TransactionGroup> groups, CancellationToken ct) => groups);

    TransactionGroupRepositoryMock
      .Setup(x => x.DeleteAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
  }

  protected virtual void SetupTransactionRepositoryMock()
  {
    TransactionRepositoryMock
      .Setup(x => x.DeleteAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
  }

  protected virtual void SetupUnitOfWorkMock()
  {
    UnitOfWorkMock
      .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);
  }

  protected virtual void SetupSmtpEmailSenderMock()
  {
    SmtpEmailSenderMock
      .Setup(x => x.SendEmailConfirmationAsync(It.IsAny<User>(), It.IsAny<string>()))
      .ReturnsAsync(Result.Success(true));
  }

  protected virtual void SetupBcryptServiceMock()
  {
    BcryptServiceMock
      .Setup(x => x.Hash(It.IsAny<string>()))
      .Returns("default_hashed_password");

    BcryptServiceMock
      .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
      .Returns(true);
  }

  protected virtual void SetupTokenServiceMock()
  {
    TokenServiceMock
      .Setup(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(Result.Success("default_token"));

    TokenServiceMock
      .Setup(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(Result.Success(true));

    TokenServiceMock
      .Setup(x => x.GetEmailFromTokenAsync(It.IsAny<string>()))
      .Returns("test@example.com");
  }

  protected virtual void SetupHttpContextAccessorMock()
  {
    var email = "test@example.com";
    var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, email) };
    var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuthType");
    var principal = new System.Security.Claims.ClaimsPrincipal(identity);
    var httpContext = new DefaultHttpContext { User = principal };
    HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
  }

  protected static Mock<ILogger<T>> CreateLoggerMock<T>()
  {
    return new Mock<ILogger<T>>();
  }

  protected virtual void ResetMocks()
  {
    UserRepositoryMock.Reset();
    UserRepositorySpecificMock.Reset();
    TransactionGroupRepositoryMock.Reset();
    TransactionRepositoryMock.Reset();
    UnitOfWorkMock.Reset();
    SmtpEmailSenderMock.Reset();
    BcryptServiceMock.Reset();
    TokenServiceMock.Reset();
  }
}
