using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.BackgroundJobs.ExchangeRate;
using Microsoft.Extensions.Hosting;

namespace FinanceApp.Application.BackgroundJobs.RabbitMQ;

public class RabbitMqConsumerServiceBackgroundJob : BackgroundService
{
  private readonly IRabbitMqClient _rabbitMqClient;
  private readonly ExchangeRateRunSignal _signal;

  public RabbitMqConsumerServiceBackgroundJob(
    IRabbitMqClient rabbitMqClient,
    ExchangeRateRunSignal signal)
  {
    _rabbitMqClient = rabbitMqClient;
    _signal = signal;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await _signal.WaitForFirstRunAsync();
    await _rabbitMqClient.SubscribeAllAsync();
    await Task.Delay(Timeout.Infinite, stoppingToken);
  }
}
