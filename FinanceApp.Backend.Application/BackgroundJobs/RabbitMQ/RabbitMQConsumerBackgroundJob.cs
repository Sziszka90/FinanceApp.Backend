using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.BackgroundJobs.ExchangeRate;
using Microsoft.Extensions.Hosting;

namespace FinanceApp.Backend.Application.BackgroundJobs.RabbitMQ;

public class RabbitMqConsumerServiceBackgroundJob : BackgroundService
{
  private readonly IRabbitMqClient _rabbitMqClient;
  private readonly ExchangeRateRunSignal _exchangeRateSignal;
  private readonly RabbitMQConsumerRunSignal _rabbitMQConsumerRunSignal;

  public RabbitMqConsumerServiceBackgroundJob(
    IRabbitMqClient rabbitMqClient,
    ExchangeRateRunSignal exchangeRateSignal,
    RabbitMQConsumerRunSignal rabbitMQConsumerRunSignal)
  {
    _rabbitMqClient = rabbitMqClient;
    _exchangeRateSignal = exchangeRateSignal;
    _rabbitMQConsumerRunSignal = rabbitMQConsumerRunSignal;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await _exchangeRateSignal.WaitForFirstRunAsync();
    await _rabbitMqClient.SubscribeAllAsync(stoppingToken);
    _rabbitMQConsumerRunSignal.SignalFirstRunCompleted();
    await Task.Delay(Timeout.Infinite, stoppingToken);
  }
}
