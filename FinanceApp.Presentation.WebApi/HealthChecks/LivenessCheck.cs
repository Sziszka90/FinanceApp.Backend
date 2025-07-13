using Microsoft.Extensions.Diagnostics.HealthChecks;
using FinanceApp.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Presentation.WebApi.HealthChecks;

public class LivenessCheck : IHealthCheck
{
  private readonly FinanceAppDbContext _dbContext;

  public LivenessCheck(FinanceAppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(
      HealthCheckContext context,
      CancellationToken cancellationToken = default)
  {
    try
    {
      await _dbContext.Database.CanConnectAsync(cancellationToken);
      return HealthCheckResult.Healthy("Liveness check passed. Database reachable.");
    }
    catch
    {
      return HealthCheckResult.Unhealthy("Liveness check failed. Database not reachable.");
    }
  }
}
