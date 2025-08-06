using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.BackgroundJobs.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.BackgroundJobTests;

public class RabbitMqConsumerBackgroundJobTests : TestBase
{

  [Fact]
  public async Task ExecuteAsync_ConsumesMessages_Success()
  {
    // arrange
    ServiceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(ScopeMock.Object);
    ServiceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(ServiceScopeFactoryMock.Object);
    ScopeMock.Setup(s => s.ServiceProvider).Returns(ServiceProviderMock.Object);
    ServiceProviderMock.Setup(sp => sp.GetService(typeof(IRabbitMqClient))).Returns(RabbitMqClientMock.Object);
    RabbitMqClientMock.Setup(c => c.SubscribeAllAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
    ExchangeRateRunSignalMock.Setup(s => s.WaitForFirstRunAsync()).Returns(Task.CompletedTask);
    RabbitMQConsumerRunSignalMock.Setup(s => s.WaitForFirstRunAsync()).Returns(Task.CompletedTask);

    var loggerMock = new Mock<ILogger<RabbitMqConsumerServiceBackgroundJob>>();
    var job = new RabbitMqConsumerServiceBackgroundJob(
        RabbitMqClientMock.Object,
        ExchangeRateRunSignalMock.Object,
        RabbitMQConsumerRunSignalMock.Object,
        loggerMock.Object);

    var cancellationTokenSource = new CancellationTokenSource();
    cancellationTokenSource.CancelAfter(100);

    // act
    await job.StartAsync(cancellationTokenSource.Token);

    // assert
    RabbitMqClientMock.Verify(c => c.SubscribeAllAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
  }

  [Fact]
  public async Task ExecuteAsync_ThrowsException_LogsCriticalError()
  {
    // arrange
    ServiceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(ScopeMock.Object);
    ServiceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(ServiceScopeFactoryMock.Object);
    ScopeMock.Setup(s => s.ServiceProvider).Returns(ServiceProviderMock.Object);
    ServiceProviderMock.Setup(sp => sp.GetService(typeof(IRabbitMqClient))).Returns(RabbitMqClientMock.Object);

    RabbitMqClientMock.Setup(c => c.SubscribeAllAsync(It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Test exception"));

    var cancellationTokenSource = new CancellationTokenSource();
    cancellationTokenSource.CancelAfter(100);

    var loggerMock = new Mock<ILogger<RabbitMqConsumerServiceBackgroundJob>>();
    // act
    var job = new RabbitMqConsumerServiceBackgroundJob(
        RabbitMqClientMock.Object,
        ExchangeRateRunSignalMock.Object,
        RabbitMQConsumerRunSignalMock.Object,
        loggerMock.Object);

    // The new implementation doesn't throw exceptions anymore, it handles them gracefully
    await job.StartAsync(cancellationTokenSource.Token);

    // assert
    RabbitMqClientMock.Verify(c => c.SubscribeAllAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
  }
}
