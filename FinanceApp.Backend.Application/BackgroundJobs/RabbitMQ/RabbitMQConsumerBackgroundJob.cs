using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.BackgroundJobs.ExchangeRate;
using FinanceApp.Backend.Application.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.BackgroundJobs.RabbitMQ;

public class RabbitMqConsumerServiceBackgroundJob : BackgroundService
{
  private readonly IRabbitMqClient _rabbitMqClient;
  private readonly ExchangeRateRunSignal _exchangeRateSignal;
  private readonly RabbitMQConsumerRunSignal _rabbitMQConsumerRunSignal;
  private readonly ILogger<RabbitMqConsumerServiceBackgroundJob> _logger;

  public RabbitMqConsumerServiceBackgroundJob(
    IRabbitMqClient rabbitMqClient,
    ExchangeRateRunSignal exchangeRateSignal,
    RabbitMQConsumerRunSignal rabbitMQConsumerRunSignal,
    ILogger<RabbitMqConsumerServiceBackgroundJob> logger)
  {
    _rabbitMqClient = rabbitMqClient;
    _exchangeRateSignal = exchangeRateSignal;
    _rabbitMQConsumerRunSignal = rabbitMQConsumerRunSignal;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await _exchangeRateSignal.WaitForFirstRunAsync();

    var retryCount = 0;
    const int maxRetries = 5;

    while (!stoppingToken.IsCancellationRequested && retryCount <= maxRetries)
    {
      try
      {
        _logger.LogInformation("Attempting to initialize RabbitMQ subscriptions (attempt {Attempt}/{MaxRetries})",
          retryCount + 1, maxRetries + 1);

        await _rabbitMqClient.SubscribeAllAsync(stoppingToken);
        _rabbitMQConsumerRunSignal.SignalFirstRunCompleted();

        _logger.LogInformation("RabbitMQ consumer service started successfully");

        await Task.Delay(Timeout.Infinite, stoppingToken);
        break;
      }
      catch (RabbitMqException ex)
      {
        retryCount++;
        _logger.LogError(ex, "RabbitMQ connection failed on attempt {Attempt}/{MaxRetries}: {Message}",
          retryCount, maxRetries + 1, ex.Message);

        if (retryCount > maxRetries)
        {
          _logger.LogCritical("Failed to establish RabbitMQ connection after {MaxRetries} attempts. " +
            "RabbitMQ consumer service will be disabled.", maxRetries + 1);
          _rabbitMQConsumerRunSignal.SignalFirstRunCompleted();
          return;
        }

        var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryCount), 60));
        _logger.LogInformation("Retrying RabbitMQ connection in {Delay} seconds...", delay.TotalSeconds);
        await Task.Delay(delay, stoppingToken);
      }
      catch (OperationCanceledException)
      {
        _logger.LogInformation("RabbitMQ consumer service was cancelled");
        throw; 
      }
      catch (Exception ex)
      {
        retryCount++;
        _logger.LogError(ex, "Unexpected error in RabbitMQ consumer service on attempt {Attempt}/{MaxRetries}: {Message}",
          retryCount, maxRetries + 1, ex.Message);

        if (retryCount > maxRetries)
        {
          _logger.LogCritical("Unexpected failures in RabbitMQ consumer service after {MaxRetries} attempts. " +
            "Service will be disabled.", maxRetries + 1);
          _rabbitMQConsumerRunSignal.SignalFirstRunCompleted();
          return;
        }

        var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryCount), 60));
        _logger.LogInformation("Retrying RabbitMQ connection in {Delay} seconds...", delay.TotalSeconds);
        await Task.Delay(delay, stoppingToken);
      }
    }
  }
}
