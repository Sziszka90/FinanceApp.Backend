using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.BackgroundJobs.ExchangeRate;
using FinanceApp.Application.Models.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceApp.Application.BackgroundJobs.RabbitMQ;

public class RabbitMqConsumerServiceBackgroundJob : BackgroundService
{
  private readonly ILogger<RabbitMqConsumerServiceBackgroundJob> _logger;
  private readonly IServiceProvider _serviceProvider;
  private IRabbitMqClient? _rabbitMqClient;
  private readonly RabbitMqSettings _rabbitMqSettings;
  private readonly ExchangeRateRunSignal _signal;

  public RabbitMqConsumerServiceBackgroundJob(
    ILogger<RabbitMqConsumerServiceBackgroundJob> logger,
    IServiceProvider serviceProvider,
    IOptions<RabbitMqSettings> rabbitMqSettings,
    ExchangeRateRunSignal signal)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;
    _rabbitMqSettings = rabbitMqSettings.Value;
    _signal = signal;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await _signal.WaitForFirstRunAsync();
    var scope = _serviceProvider.CreateScope();
    _rabbitMqClient = scope.ServiceProvider.GetRequiredService<IRabbitMqClient>();

    await _rabbitMqClient.InitializeAsync();
    await _rabbitMqClient.SubscribeAllAsync();
    await Task.Delay(Timeout.Infinite, stoppingToken);
  }
}
