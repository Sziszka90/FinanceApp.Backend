using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.BackgroundJobs.ExchangeRate;
using FinanceApp.Backend.Application.BackgroundJobs.RabbitMQ;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit;

public abstract class TestBase
{
  protected readonly Mock<IUserRepository> UserRepositoryMock = new Mock<IUserRepository>();
  protected readonly Mock<IUserRepository> UserRepositorySpecificMock = new Mock<IUserRepository>();
  protected readonly Mock<ITransactionGroupRepository> TransactionGroupRepositoryMock = new Mock<ITransactionGroupRepository>();
  protected readonly Mock<ITransactionRepository> TransactionRepositoryMock = new Mock<ITransactionRepository>();
  protected readonly Mock<IExchangeRateRepository> ExchangeRateRepositoryMock = new Mock<IExchangeRateRepository>();
  protected readonly Mock<IUnitOfWork> UnitOfWorkMock = new Mock<IUnitOfWork>();
  protected readonly Mock<ISmtpEmailSender> SmtpEmailSenderMock = new Mock<ISmtpEmailSender>();
  protected readonly Mock<IBcryptService> BcryptServiceMock = new Mock<IBcryptService>();
  protected readonly Mock<ITokenService> TokenServiceMock = new Mock<ITokenService>();
  protected readonly Mock<IUserService> UserServiceMock = new Mock<IUserService>();
  protected readonly Mock<IExchangeRateService> ExchangeRateServiceMock = new Mock<IExchangeRateService>();
  protected readonly Mock<IHttpContextAccessor> HttpContextAccessorMock = new Mock<IHttpContextAccessor>();
  protected readonly Mock<ILogger<object>> LoggerMock = new Mock<ILogger<object>>();
  protected readonly Mock<IServiceProvider> ServiceProviderMock = new Mock<IServiceProvider>();
  protected readonly Mock<IServiceScope> ScopeMock = new Mock<IServiceScope>();
  protected readonly Mock<IServiceScopeFactory> ServiceScopeFactoryMock = new Mock<IServiceScopeFactory>();
  protected readonly Mock<IExchangeRateClient> ExchangeRateClientMock = new Mock<IExchangeRateClient>();
  protected readonly Mock<ExchangeRateRunSignal> ExchangeRateRunSignalMock = new Mock<ExchangeRateRunSignal>();
  protected readonly Mock<RabbitMQConsumerRunSignal> RabbitMQConsumerRunSignalMock = new Mock<RabbitMQConsumerRunSignal>();
  protected readonly Mock<IRabbitMqClient> RabbitMqClientMock = new Mock<IRabbitMqClient>();
  protected readonly Mock<HttpMessageHandler> HttpMessageHandlerMock = new Mock<HttpMessageHandler>();
  protected readonly Mock<IExchangeRateCacheManager> ExchangeRateCacheManagerMock = new Mock<IExchangeRateCacheManager>();
  protected readonly IMapper Mapper;

  protected TestBase()
  {
    UserRepositoryMock = new Mock<IUserRepository>();
    UserRepositorySpecificMock = new Mock<IUserRepository>();
    TransactionGroupRepositoryMock = new Mock<ITransactionGroupRepository>();
    TransactionRepositoryMock = new Mock<ITransactionRepository>();
    ExchangeRateRepositoryMock = new Mock<IExchangeRateRepository>();
    UnitOfWorkMock = new Mock<IUnitOfWork>();
    SmtpEmailSenderMock = new Mock<ISmtpEmailSender>();
    BcryptServiceMock = new Mock<IBcryptService>();
    TokenServiceMock = new Mock<ITokenService>();
    HttpContextAccessorMock = new Mock<IHttpContextAccessor>();
    ExchangeRateCacheManagerMock = new Mock<IExchangeRateCacheManager>();

    Mapper = new MapperConfiguration(cfg =>
    {
      cfg.CreateMap<User, GetUserDto>();
      cfg.CreateMap<Transaction, GetTransactionDto>();
      cfg.CreateMap<UpdateTransactionDto, Transaction>();
      cfg.CreateMap<CreateTransactionDto, Transaction>();
      cfg.CreateMap<TransactionGroup, GetTransactionGroupDto>();
      cfg.CreateMap<CreateTransactionGroupDto, TransactionGroup>();
      cfg.CreateMap<UpdateTransactionGroupDto, TransactionGroup>();
    }).CreateMapper();

    SetupBcryptServiceMock();
    SetupTokenServiceMock();
    SetupHttpContextAccessorMock();
    SetupUserRepositoryMock();
    SetupTransactionGroupRepositoryMock();
    SetupTransactionRepositoryMock();
    SetupUnitOfWorkMock();
    SetupSmtpEmailSenderMock();
    SetupExchangeRateRepositoryMock();
    SetupUserServiceMock();
    SetupExchangeRateCacheManagerMock();
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

  protected virtual void SetupExchangeRateRepositoryMock()
  {
    ExchangeRateRepositoryMock
      .Setup(x => x.GetExchangeRatesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync([
        new ExchangeRate("USD", "EUR", 0.85m),
        new ExchangeRate("EUR", "USD", 1.18m),
        new ExchangeRate("USD", "GBP", 0.75m),
        new ExchangeRate("GBP", "USD", 1.33m),
        new ExchangeRate("USD", "JPY", 110.0m),
        new ExchangeRate("JPY", "USD", 0.0091m)
      ]);
  }

  protected virtual void SetupTransactionRepositoryMock()
  {
    TransactionRepositoryMock
      .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Transaction?)null);

    TransactionRepositoryMock
      .Setup(x => x.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Transaction?)null!);

    TransactionRepositoryMock
      .Setup(x => x.DeleteAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    TransactionRepositoryMock
      .Setup(x => x.GetTransactionsByTopTransactionGroups(
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<Guid>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction>());
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
      .Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<string>()))
      .ReturnsAsync(Result.Success("refresh_token"));

    TokenServiceMock
      .Setup(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
      .ReturnsAsync(Result.Success(true));

    TokenServiceMock
      .Setup(x => x.GetEmailFromToken(It.IsAny<string>()))
      .Returns("test@example.com");
  }

  protected virtual void SetupUserServiceMock()
  {
    UserServiceMock
      .Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD)));

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

  protected virtual void SetupExchangeRateCacheManagerMock()
  {
    ExchangeRateCacheManagerMock
      .Setup(x => x.GetRateAsync(It.IsAny<DateTimeOffset>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(1.0m));
  }

  protected virtual void SetupExchangeRateServiceMock()
  {
    ExchangeRateServiceMock
      .Setup(x => x.ConvertAmountAsync(It.IsAny<decimal>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(1.0m));
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
