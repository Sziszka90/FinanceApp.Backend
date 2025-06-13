using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FinanceApp.Presentation.WebApi.HealthChecks;

public class StartupCheck : IHealthCheck
{
  public Task<HealthCheckResult> CheckHealthAsync(
      HealthCheckContext context,
      CancellationToken cancellationToken = default)
  {

    bool appIsReady = true;

    if (appIsReady)
    {
      return Task.FromResult(HealthCheckResult.Healthy("Startup check passed."));
    }

    return Task.FromResult(HealthCheckResult.Unhealthy("Startup check failed."));
  }
}
