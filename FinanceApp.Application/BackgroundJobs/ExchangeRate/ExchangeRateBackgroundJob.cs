using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.BackgroundJobs.ExchangeRate;


public class ExchangeRateBackgroundJob : BackgroundService
{
  private readonly ILogger<ExchangeRateBackgroundJob> _logger;
  private readonly IServiceProvider _serviceProvider;

  public ExchangeRateBackgroundJob(
    ILogger<ExchangeRateBackgroundJob> logger,
    IServiceProvider serviceProvider)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;
  }

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      int retryCount = 0;
      const int maxRetries = 3;
      TimeSpan delay = TimeSpan.FromSeconds(10);
      bool success = false;

      while (retryCount < maxRetries && !success && !cancellationToken.IsCancellationRequested)
      {
        using var scope = _serviceProvider.CreateScope();

        var exchangeRateRepository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var exchangeRateClient = scope.ServiceProvider.GetRequiredService<IExchangeRateClient>();
        var rates = await exchangeRateClient.GetExchangeRatesAsync();

        if (rates.IsSuccess)
        {
          await exchangeRateRepository.BatchCreateExchangeRatesAsync(rates.Data!, cancellationToken);
          await unitOfWork.SaveChangesAsync(cancellationToken);
          _logger.LogDebug("Exchange rates updated successfully.");
          success = true;
        }
        else
        {
          retryCount++;
          _logger.LogError("Error occurred while fetching exchange rates. Retry {Retry}/{MaxRetries}.", retryCount, maxRetries);
          if (retryCount < maxRetries)
          {
            await Task.Delay(delay, cancellationToken);
            delay = delay * 2; // Exponential backoff
          }
        }
      }
      await Task.Delay(TimeSpan.FromDays(7), cancellationToken);
    }
  }
}
