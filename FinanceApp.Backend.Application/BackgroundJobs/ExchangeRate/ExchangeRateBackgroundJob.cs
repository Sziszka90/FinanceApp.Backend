using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

namespace FinanceApp.Backend.Application.BackgroundJobs.ExchangeRate;


public class ExchangeRateBackgroundJob : BackgroundService
{
  private readonly ILogger<ExchangeRateBackgroundJob> _logger;
  private readonly IServiceProvider _serviceProvider;
  private readonly ExchangeRateRunSignal _signal;
  private readonly IAsyncPolicy _retryPolicy;
  private readonly IExchangeRateCacheManager _exchangeRateCacheManager;

  public ExchangeRateBackgroundJob(
    ILogger<ExchangeRateBackgroundJob> logger,
    IServiceProvider serviceProvider,
    ExchangeRateRunSignal signal,
    IExchangeRateCacheManager exchangeRateCacheManager)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;
    _signal = signal;
    _exchangeRateCacheManager = exchangeRateCacheManager;

    _retryPolicy = Policy
      .Handle<Exception>()
      .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), 30)),
        onRetry: (exception, timespan, retryCount, context) =>
        {
          _logger.LogWarning(exception,
            "Failed exchange rate sync operation on attempt {Attempt}/3. Retrying in {Delay} seconds...",
            retryCount, timespan.TotalSeconds);
        });
  }

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    try
    {
      using var scope = _serviceProvider.CreateScope();

      var exchangeRateRepository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var exchangeRateClient = scope.ServiceProvider.GetRequiredService<IExchangeRateClient>();

      while (!cancellationToken.IsCancellationRequested)
      {
        try
        {
          await _retryPolicy.ExecuteAsync(async () =>
          {
            var actualRates = await exchangeRateRepository.GetActualExchangeRatesAsync(cancellationToken);

            foreach (var rate in actualRates)
            {
              rate.ExpireExchangeRate();
            }

            var rates = await exchangeRateClient.GetExchangeRatesAsync();

            await exchangeRateRepository.BatchCreateExchangeRatesAsync(rates.Data!, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Exchange rates updated successfully.");

            var allRates = await exchangeRateRepository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);
            var cacheResult = await _exchangeRateCacheManager.CacheAllRatesAsync(allRates, cancellationToken);

            if (!cacheResult.IsSuccess)
            {
              _logger.LogError("Failed to cache exchange rates: {Error}", cacheResult.ApplicationError?.Message);
              throw new Exception("Failed to cache exchange rates.");
            }
            _signal.SignalFirstRunCompleted();
          });
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to sync exchange rates after all retry attempts. Stopping application.");
          throw;
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
      _logger.LogError(ex, "Critical error in Exchange Rate Background Job. Service will stop.");
      throw;
    }
  }
}
