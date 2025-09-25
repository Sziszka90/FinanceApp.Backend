using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
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

    ExchangeRateRepositoryMock.Setup(r => r.GetActualExchangeRatesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<ExchangeRate>());

    ExchangeRateClientMock.Setup(c => c.GetExchangeRatesAsync())
        .ReturnsAsync(Result<List<ExchangeRate>>.Success(new List<ExchangeRate>()));

    UnitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    // act
    var job = new ExchangeRateBackgroundJob(
        _loggerMock.Object,
        ServiceProviderMock.Object,
        ExchangeRateRunSignalMock.Object);

    var cancellationTokenSource = new CancellationTokenSource();
    cancellationTokenSource.CancelAfter(100);

    await job.StartAsync(cancellationTokenSource.Token);

    // assert
    UnitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    ExchangeRateRunSignalMock.Verify(s => s.SignalFirstRunCompleted(), Times.AtLeastOnce());
  }
}
