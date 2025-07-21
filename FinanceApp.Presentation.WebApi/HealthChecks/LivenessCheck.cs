using Microsoft.Extensions.Diagnostics.HealthChecks;
using FinanceApp.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Presentation.WebApi.HealthChecks;

public class LivenessCheck : IHealthCheck
{
  private readonly ILogger<LivenessCheck> _logger;
  private readonly FinanceAppDbContext _dbContext;

  public LivenessCheck(ILogger<LivenessCheck> logger,FinanceAppDbContext dbContext)
  {
    _logger = logger;
    _dbContext = dbContext;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(
      HealthCheckContext context,
      CancellationToken cancellationToken = default)
  {
    try
    {
      await _dbContext.Database.CanConnectAsync(cancellationToken);
      _logger.LogInformation("Liveness check passed. Database is reachable.");
      return HealthCheckResult.Healthy("Liveness check passed. Database reachable.");
    }
    catch
    {
      _logger.LogWarning("Liveness check failed. Database not reachable.");
      return HealthCheckResult.Unhealthy("Liveness check failed. Database not reachable.");
    }
  }
}
