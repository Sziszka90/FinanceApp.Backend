using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.BackgroundJobs.ExchangeRate;


public class ExchangeRateBackgroundJob : BackgroundService
{
  private readonly ILogger<ExchangeRateBackgroundJob> _logger;
  private readonly IServiceProvider _serviceProvider;
  private readonly ExchangeRateRunSignal _signal;

  public ExchangeRateBackgroundJob(
    ILogger<ExchangeRateBackgroundJob> logger,
    IServiceProvider serviceProvider,
    ExchangeRateRunSignal signal)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;
    _signal = signal;
  }

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    try
    {
      using var scope = _serviceProvider.CreateScope();

      var exchangeRateRepository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var exchangeRateClient = scope.ServiceProvider.GetRequiredService<IExchangeRateClient>();

      try
      {
        var existingRates = await exchangeRateRepository.GetExchangeRatesAsync(noTracking: false, cancellationToken);

        if (existingRates.Count > 0)
        {
          exchangeRateRepository.DeleteAllAsync(existingRates, cancellationToken);
          await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Exchange rates clean up completed.");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to clean up existing exchange rates. Continuing with sync process.");
      }

      while (!cancellationToken.IsCancellationRequested)
      {
        int retryCount = 0;
        const int MAX_RETRIES = 3;
        TimeSpan delay = TimeSpan.FromSeconds(10);
        bool success = false;

        while (retryCount < MAX_RETRIES && !success && !cancellationToken.IsCancellationRequested)
        {
          try
          {
            var rates = await exchangeRateClient.GetExchangeRatesAsync();

            if (rates.IsSuccess)
            {
              await exchangeRateRepository.BatchCreateExchangeRatesAsync(rates.Data!, cancellationToken);
              await unitOfWork.SaveChangesAsync(cancellationToken);
              _logger.LogInformation("Exchange rates updated successfully.");
              _signal.SignalFirstRunCompleted();
              success = true;
            }
            else
            {
              retryCount++;
              _logger.LogError("Error occurred while fetching exchange rates. Retry {Retry}/{MaxRetries}.", retryCount, MAX_RETRIES);
              if (retryCount < MAX_RETRIES)
              {
                await Task.Delay(delay, cancellationToken);
                delay = delay * 2; // Exponential backoff
              }
            }
          }
          catch (Exception ex)
          {
            retryCount++;
            _logger.LogError(ex, "Exception during exchange rate sync attempt {Retry}/{MaxRetries}: {Message}", retryCount, MAX_RETRIES, ex.Message);

            if (retryCount < MAX_RETRIES)
            {
              await Task.Delay(delay, cancellationToken);
              delay = delay * 2; // Exponential backoff
            }
          }
        }

        if (!success)
        {
          _logger.LogWarning("Failed to sync exchange rates after {MaxRetries} attempts. Will retry in 7 days.", MAX_RETRIES);
        }

        await Task.Delay(TimeSpan.FromDays(7), cancellationToken);
      }
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Exchange rate background job was cancelled.");
    }
    catch (Exception ex)
    {
      _logger.LogCritical(ex, "Critical error in Exchange Rate Background Job. Service will stop.");
      throw;
    }
  }
}
