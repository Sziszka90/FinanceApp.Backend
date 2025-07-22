using Microsoft.Extensions.Diagnostics.HealthChecks;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Backend.Presentation.WebApi.HealthChecks;

public class ReadinessCheck : IHealthCheck
{
  private readonly ILogger<ReadinessCheck> _logger;
  private readonly FinanceAppDbContext _dbContext;

  public ReadinessCheck(ILogger<ReadinessCheck> logger, FinanceAppDbContext dbContext)
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
      _logger.LogInformation("Readiness check passed. Database is reachable.");
      return HealthCheckResult.Healthy("Ready. Database reachable.");
    }
    catch
    {
      _logger.LogWarning("Readiness check failed. Database not reachable.");
      return HealthCheckResult.Unhealthy("Not ready. Database not reachable.");
    }
  }
}
