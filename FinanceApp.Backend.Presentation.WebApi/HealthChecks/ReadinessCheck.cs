using Microsoft.Extensions.Diagnostics.HealthChecks;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using FinanceApp.Backend.Application.Abstraction.Clients;

namespace FinanceApp.Backend.Presentation.WebApi.HealthChecks;

public class ReadinessCheck : IHealthCheck
{
  private readonly ILogger<ReadinessCheck> _logger;
  private readonly FinanceAppDbContext _dbContext;
  private readonly IRabbitMqConnectionManager _rabbitMqConnectionManager;

  public ReadinessCheck(
    ILogger<ReadinessCheck> logger,
    FinanceAppDbContext dbContext,
    IRabbitMqConnectionManager rabbitMqConnectionManager)
  {
    _logger = logger;
    _dbContext = dbContext;
    _rabbitMqConnectionManager = rabbitMqConnectionManager;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(
      HealthCheckContext context,
      CancellationToken cancellationToken = default)
  {
    var healthData = new Dictionary<string, object>();
    var failures = new List<string>();

    try
    {
      await _dbContext.Database.CanConnectAsync(cancellationToken);
      healthData.Add("database", "Connected");
      _logger.LogDebug("Database health check passed.");
    }
    catch (Exception ex)
    {
      healthData.Add("database", "Disconnected");
      failures.Add("Database not reachable");
      _logger.LogWarning(ex, "Database health check failed.");
    }

    try
    {
      var channel = _rabbitMqConnectionManager.Channel;
      if (channel != null && channel.IsOpen)
      {
        healthData.Add("rabbitmq", "Connected");
        _logger.LogDebug("RabbitMQ health check passed.");
      }
      else
      {
        healthData.Add("rabbitmq", "Disconnected");
        failures.Add("RabbitMQ channel not available or closed");
        _logger.LogWarning("RabbitMQ health check failed - channel not available or closed.");
      }
    }
    catch (Exception ex)
    {
      healthData.Add("rabbitmq", "Error");
      failures.Add("RabbitMQ health check error");
      _logger.LogWarning(ex, "RabbitMQ health check failed with exception.");
    }

    if (failures.Count == 0)
    {
      _logger.LogInformation("Readiness check passed. All services are healthy.");
      return HealthCheckResult.Healthy("Ready. All services are healthy.", healthData);
    }
    else
    {
      var failureMessage = string.Join("; ", failures);
      _logger.LogWarning("Readiness check failed: {Failures}", failureMessage);
      return HealthCheckResult.Unhealthy($"Not ready: {failureMessage}", data: healthData);
    }
  }
}
