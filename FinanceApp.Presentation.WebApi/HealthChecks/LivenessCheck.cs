using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FinanceApp.Presentation.WebApi.HealthChecks;

public class LivenessCheck : IHealthCheck
{
  public Task<HealthCheckResult> CheckHealthAsync(
      HealthCheckContext context,
      CancellationToken cancellationToken = default)
  {
    return Task.FromResult(HealthCheckResult.Healthy("Liveness check passed."));
  }
}
