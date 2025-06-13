using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FinanceApp.Presentation.WebApi.HealthChecks;

public class ReadinessCheck : IHealthCheck
{
  public Task<HealthCheckResult> CheckHealthAsync(
      HealthCheckContext context,
      CancellationToken cancellationToken = default)
  {
    bool dbConnectionOk = true; // replace with actual DB check
    return dbConnectionOk
        ? Task.FromResult(HealthCheckResult.Healthy("Ready."))
        : Task.FromResult(HealthCheckResult.Unhealthy("Not ready."));
  }
}
