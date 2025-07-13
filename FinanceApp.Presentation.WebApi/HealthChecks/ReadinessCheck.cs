using Microsoft.Extensions.Diagnostics.HealthChecks;
using FinanceApp.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Presentation.WebApi.HealthChecks;

public class ReadinessCheck : IHealthCheck
{
  private readonly FinanceAppDbContext _dbContext;

  public ReadinessCheck(FinanceAppDbContext dbContext)
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
      return HealthCheckResult.Healthy("Ready. Database reachable.");
    }
    catch
    {
      return HealthCheckResult.Unhealthy("Not ready. Database not reachable.");
    }
  }
}
