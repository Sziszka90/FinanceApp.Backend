using FinanceApp.Backend.Application.Abstraction.Clients;
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
      IRabbitMqClient rabbitMqClient,
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
    bool dbReady = await _dbContext.Database.CanConnectAsync();
    bool consumerJobExecuted = _rabbitMQConsumerRunSignal.HasRun;

    if (dbReady && consumerJobExecuted)
    {
      _logger.LogInformation("Startup check passed.");
      return HealthCheckResult.Healthy("Startup check passed.");
    }
    else
    {
      _logger.LogWarning("Startup check failed. Consumer job executed: {ConsumerJobExecuted}", consumerJobExecuted);
      return HealthCheckResult.Unhealthy("Startup check failed. Consumer job executed: " + consumerJobExecuted);
    }
  }
}
