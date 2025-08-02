using FinanceApp.Backend.Application.BackgroundJobs.ExchangeRate;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Testing.Unit.BackgroundJobTests;

public class ExchangeRateBackgroundJobTests : TestBase
{
  public ExchangeRateBackgroundJobTests()
  {
    var loggerMock = new Mock<ILogger<ExchangeRateBackgroundJob>>();
    var serviceProviderMock = new Mock<IServiceProvider>();
    var scopeMock = new Mock<IServiceScope>();
    var exchangeRateRepositoryMock = new Mock<IExchangeRateRepository>();
    var unitOfWorkMock = new Mock<IUnitOfWork>();
    var exchangeRateClientMock = new Mock<IExchangeRateClient>();
    var signalMock = new Mock<ExchangeRateRunSignal>();
  }

  [Fact]
  public async Task ExecuteAsync_UpdatesExchangeRates_Success()
  {
    // arrange


    var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
    serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
    serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
    scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
    serviceProviderMock.Setup(sp => sp.GetRequiredService<IExchangeRateRepository>()).Returns(exchangeRateRepositoryMock.Object);
    serviceProviderMock.Setup(sp => sp.GetRequiredService<IUnitOfWork>()).Returns(unitOfWorkMock.Object);
    serviceProviderMock.Setup(sp => sp.GetRequiredService<IExchangeRateClient>()).Returns(exchangeRateClientMock.Object);

    // Setup repository and client
    exchangeRateRepositoryMock.Setup(r => r.GetExchangeRatesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<ExchangeRate>());

    exchangeRateClientMock.Setup(c => c.GetExchangeRatesAsync())
        .ReturnsAsync(Result<List<ExchangeRate>>.Success(new List<ExchangeRate>()));

    // Setup unit of work
    unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    // Setup signal
    signalMock.Setup(s => s.SignalFirstRunCompleted());

    var job = new ExchangeRateBackgroundJob(
        loggerMock.Object,
        serviceProviderMock.Object,
        signalMock.Object);

    var cancellationTokenSource = new CancellationTokenSource();
    cancellationTokenSource.CancelAfter(100); // Cancel quickly for test

    // act
    await job.StartAsync(cancellationTokenSource.Token);

    // assert
    exchangeRateRepositoryMock.Verify(r => r.GetExchangeRatesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    exchangeRateClientMock.Verify(c => c.GetExchangeRatesAsync(), Times.AtLeastOnce());
    unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    signalMock.Verify(s => s.SignalFirstRunCompleted(), Times.AtLeastOnce());
  }
}
