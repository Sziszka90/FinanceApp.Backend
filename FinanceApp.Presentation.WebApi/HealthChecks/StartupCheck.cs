using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FinanceApp.Presentation.WebApi.HealthChecks;

public class StartupCheck : IHealthCheck
{
  private readonly ILogger<StartupCheck> _logger;

  public StartupCheck(ILogger<StartupCheck> logger)
  {
    _logger = logger;
  }
  public Task<HealthCheckResult> CheckHealthAsync(
      HealthCheckContext context,
      CancellationToken cancellationToken = default)
  {
    bool appIsReady = true;

    if (appIsReady)
    {
      _logger.LogInformation("Startup check passed.");
      return Task.FromResult(HealthCheckResult.Healthy("Startup check passed."));
    }

    _logger.LogWarning("Startup check failed.");
    return Task.FromResult(HealthCheckResult.Unhealthy("Startup check failed."));
  }
}
