using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.BackgroundJobs.ExchangeRate;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.BackgroundJobTests;

public class ExchangeRateBackgroundJobTests : TestBase
{
  private readonly Mock<ILogger<ExchangeRateBackgroundJob>> _loggerMock = new();
  public ExchangeRateBackgroundJobTests()
  {

  }

  [Fact]
  public async Task ExecuteAsync_UpdatesExchangeRates_Success()
  {
    // arrange
    ServiceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(ScopeMock.Object);
    ServiceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(ServiceScopeFactoryMock.Object);
    ScopeMock.Setup(s => s.ServiceProvider).Returns(ServiceProviderMock.Object);
    ServiceProviderMock.Setup(sp => sp.GetService(typeof(IExchangeRateRepository))).Returns(ExchangeRateRepositoryMock.Object);
    ServiceProviderMock.Setup(sp => sp.GetService(typeof(IUnitOfWork))).Returns(UnitOfWorkMock.Object);
    ServiceProviderMock.Setup(sp => sp.GetService(typeof(IExchangeRateClient))).Returns(ExchangeRateClientMock.Object);
    var refreshState = new ExchangeRateRefreshState();
    var refreshStateRepository = new Mock<IRepository<ExchangeRateRefreshState>>();
    refreshStateRepository
      .Setup(repository => repository.GetByIdAsync(
        ExchangeRateRefreshState.SingletonId,
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(refreshState);
    ServiceProviderMock.Setup(sp => sp.GetService(typeof(IRepository<ExchangeRateRefreshState>)))
      .Returns(refreshStateRepository.Object);

    ExchangeRateRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<ExchangeRate>());
    ExchangeRateRepositoryMock.Setup(r => r.GetActualExchangeRatesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<ExchangeRate>());

    ExchangeRateClientMock.Setup(c => c.GetExchangeRatesAsync())
        .ReturnsAsync(Result<List<ExchangeRate>>.Success(new List<ExchangeRate>
        {
          new ExchangeRate("USD", "EUR", 0.85m)
        }));

    UnitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    ExchangeRateCacheManagerMock.Setup(c => c.CacheAllRatesAsync(It.IsAny<List<ExchangeRate>>(), It.IsAny<CancellationToken>()))
        .Returns(Task.FromResult(Result.Success()));

    // act
    ServiceProviderMock.Setup(sp => sp.GetService(typeof(IExchangeRateCacheManager))).Returns(ExchangeRateCacheManagerMock.Object);

    var lease = new Mock<IExchangeRateRefreshLease>();
    lease.SetupGet(item => item.LostToken).Returns(CancellationToken.None);
    var refreshLock = new Mock<IExchangeRateRefreshLock>();
    refreshLock
      .Setup(item => item.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(lease.Object);

    var job = new ExchangeRateBackgroundJob(
        _loggerMock.Object,
        ServiceProviderMock.Object,
        ExchangeRateRunSignalMock.Object,
        ExchangeRateCacheManagerMock.Object,
        refreshLock.Object);

    var cancellationTokenSource = new CancellationTokenSource();

    await job.StartAsync(cancellationTokenSource.Token);

    var completedTask = await Task.WhenAny(
        Task.Delay(5000),
        Task.Run(async () =>
        {
          await Task.Delay(500);
          cancellationTokenSource.Cancel();
        })
    );

    // assert
    UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    ExchangeRateRunSignalMock.Verify(s => s.SignalFirstRunCompleted(), Times.AtLeastOnce());
  }
}
