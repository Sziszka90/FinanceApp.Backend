using FinanceApp.Backend.Application.BackgroundJobs.RabbitMQ;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FinanceApp.Backend.Presentation.WebApi.HealthChecks;

public class StartupCheck : IHealthCheck
{
  private readonly ILogger<StartupCheck> _logger;
  private readonly FinanceAppDbContext _dbContext;
  private readonly RabbitMQConsumerRunSignal _rabbitMQConsumerRunSignal;

  public StartupCheck(
    ILogger<StartupCheck> logger,
    FinanceAppDbContext dbContext,
    RabbitMQConsumerRunSignal rabbitMQConsumerRunSignal)
  {
    _logger = logger;
    _dbContext = dbContext;
    _rabbitMQConsumerRunSignal = rabbitMQConsumerRunSignal;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(
    HealthCheckContext context,
    CancellationToken cancellationToken = default)
  {
    var healthData = new Dictionary<string, object>();
    var failures = new List<string>();

    try
    {
      var dbReady = await _dbContext.Database.CanConnectAsync(cancellationToken);
      healthData.Add("database", dbReady ? "Ready" : "Not Ready");

      if (dbReady)
      {
        _logger.LogDebug("Database startup check passed.");
      }
      else
      {
        failures.Add("Database not ready");
        _logger.LogWarning("Database startup check failed - cannot connect.");
      }
    }
    catch (Exception ex)
    {
      healthData.Add("database", "Error");
      failures.Add("Database connection error");
      _logger.LogWarning(ex, "Database startup check failed with exception.");
    }

    var consumerJobExecuted = _rabbitMQConsumerRunSignal.HasRun;
    healthData.Add("rabbitmq_consumer", consumerJobExecuted ? "Initialized" : "Not Initialized");

    if (consumerJobExecuted)
    {
      _logger.LogDebug("RabbitMQ consumer startup check passed.");
    }
    else
    {
      failures.Add("RabbitMQ consumer not initialized");
      _logger.LogWarning("RabbitMQ consumer startup check failed - consumer job not executed.");
    }

    if (failures.Count == 0)
    {
      _logger.LogInformation("Startup check passed. All components are ready.");
      return HealthCheckResult.Healthy("Startup check passed. All components are ready.", healthData);
    }
    else
    {
      var failureMessage = string.Join("; ", failures);
      _logger.LogWarning("Startup check failed: {Failures}", failureMessage);
      return HealthCheckResult.Unhealthy($"Startup check failed: {failureMessage}", data: healthData);
    }
  }
}
