using FinanceApp.Application.Abstraction.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.BackgroundJobs;

public class ExchangeRateBackgroundJob : BackgroundService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<ExchangeRateBackgroundJob> _logger;

  public ExchangeRateBackgroundJob(IServiceProvider serviceProvider, ILogger<ExchangeRateBackgroundJob> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        using var scope = _serviceProvider.CreateScope();
        var exchangeRateClient = scope.ServiceProvider.GetRequiredService<IExchangeRateClient>();
        //var rates = await exchangeRateClient.GetExchangeRateAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while fetching exchange rates.");
      }
      // Wait for 1 week before next run
      await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
    }
  }
}
