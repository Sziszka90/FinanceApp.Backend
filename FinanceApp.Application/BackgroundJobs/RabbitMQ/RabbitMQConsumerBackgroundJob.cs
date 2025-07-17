using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Models.Options;
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

  public RabbitMqConsumerServiceBackgroundJob(
    ILogger<RabbitMqConsumerServiceBackgroundJob> logger,
    IServiceProvider serviceProvider,
    IOptions<RabbitMqSettings> rabbitMqSettings)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;
    _rabbitMqSettings = rabbitMqSettings.Value;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
     var scope = _serviceProvider.CreateScope();
     _rabbitMqClient = scope.ServiceProvider.GetRequiredService<IRabbitMqClient>();

      await _rabbitMqClient.InitializeAsync();
      await _rabbitMqClient.SubscribeAsync(_rabbitMqSettings.Queues.TransactionsMatched);
      await Task.Delay(Timeout.Infinite, stoppingToken);
  }
}
