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

  }

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    try
    {
      DateTimeOffset nextRefreshAt;
      // Saved snapshots remain usable when the provider is temporarily unavailable.
      using (var startupScope = _serviceProvider.CreateScope())
      {
        var repository = startupScope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
        var savedRates = await repository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);
        nextRefreshAt = savedRates.Count == 0
          ? DateTimeOffset.UtcNow
          : savedRates.Max(rate => rate.ValidFrom).AddDays(7);
        try
        {
          await _exchangeRateCacheManager.CacheAllRatesAsync(savedRates, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
          _logger.LogWarning(ex, "Could not warm exchange rate cache; database lookup remains available");
        }
      }
      _signal.SignalFirstRunCompleted();

      while (!cancellationToken.IsCancellationRequested)
      {
        var wait = nextRefreshAt - DateTimeOffset.UtcNow;
        if (wait > TimeSpan.Zero)
        {
          await Task.Delay(wait, cancellationToken);
        }

        try
        {
          using var scope = _serviceProvider.CreateScope();
          var exchangeRateRepository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
          var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
          var exchangeRateClient = scope.ServiceProvider.GetRequiredService<IExchangeRateClient>();

          // Fetch and validate first so a provider failure never expires the saved active rates.
          var rates = await exchangeRateClient.GetExchangeRatesAsync();
          if (!rates.IsSuccess || rates.Data is null || rates.Data.Count == 0)
          {
            throw new InvalidDataException($"Exchange rate provider returned no usable rates: {rates.ApplicationError?.Message}");
          }

          var actualRates = await exchangeRateRepository.GetActualExchangeRatesAsync(cancellationToken);

          foreach (var rate in actualRates)
          {
            rate.ExpireExchangeRate();
          }

          await exchangeRateRepository.BatchCreateExchangeRatesAsync(rates.Data, cancellationToken);
          await unitOfWork.SaveChangesAsync(cancellationToken);
          _logger.LogInformation("Exchange rates updated successfully.");
          nextRefreshAt = DateTimeOffset.UtcNow.AddDays(7);

          try
          {
            var allRates = await exchangeRateRepository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);
            await _exchangeRateCacheManager.CacheAllRatesAsync(allRates, cancellationToken);
          }
          catch (Exception ex) when (ex is not OperationCanceledException)
          {
            _logger.LogWarning(ex, "Saved exchange rates but could not refresh cache; database lookup remains available");
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to sync exchange rates; retrying tomorrow with saved snapshots.");
          nextRefreshAt = DateTimeOffset.UtcNow.AddDays(1);
          continue;
        }
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
