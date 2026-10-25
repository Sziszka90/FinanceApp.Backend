using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.BackgroundJobs.ExchangeRate;

public class ExchangeRateBackgroundJob : BackgroundService
{
  private static readonly TimeSpan _refreshInterval = TimeSpan.FromDays(7);
  private static readonly TimeSpan _lockLeaseDuration = TimeSpan.FromMinutes(2);
  private static readonly TimeSpan _lockRetryDelay = TimeSpan.FromSeconds(30);

  private readonly ILogger<ExchangeRateBackgroundJob> _logger;
  private readonly IServiceProvider _serviceProvider;
  private readonly ExchangeRateRunSignal _signal;
  private readonly IExchangeRateCacheManager _exchangeRateCacheManager;
  private readonly IExchangeRateRefreshLock _refreshLock;

  public ExchangeRateBackgroundJob(
    ILogger<ExchangeRateBackgroundJob> logger,
    IServiceProvider serviceProvider,
    ExchangeRateRunSignal signal,
    IExchangeRateCacheManager exchangeRateCacheManager,
    IExchangeRateRefreshLock refreshLock)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;
    _signal = signal;
    _exchangeRateCacheManager = exchangeRateCacheManager;
    _refreshLock = refreshLock;
  }

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    try
    {
      await WarmCacheAndSignalStartupAsync(cancellationToken);
      await InitializeRefreshScheduleAsync(cancellationToken);

      while (!cancellationToken.IsCancellationRequested)
      {
        var nextAttemptAt = await GetNextAttemptAtAsync(cancellationToken);
        var wait = nextAttemptAt - DateTimeOffset.UtcNow;
        if (wait > TimeSpan.Zero)
        {
          await Task.Delay(wait, cancellationToken);
        }

        IExchangeRateRefreshLease? lease;
        try
        {
          lease = await _refreshLock.TryAcquireAsync(_lockLeaseDuration, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
          _logger.LogError(ex, "Could not acquire the exchange-rate refresh lease; retrying in {RetryDelay}", _lockRetryDelay);
          await Task.Delay(_lockRetryDelay, cancellationToken);
          continue;
        }

        if (lease is null)
        {
          // Another replica owns the refresh. Re-read shared state shortly instead of starting a local weekly timer.
          await Task.Delay(_lockRetryDelay, cancellationToken);
          continue;
        }

        await using (lease)
        {
          try
          {
            await RefreshIfDueAsync(lease, cancellationToken);
          }
          catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
          {
            throw;
          }
          catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
          {
            _logger.LogWarning("Lost the exchange-rate refresh lease; another replica may retry after the lease expires.");
            await Task.Delay(_lockRetryDelay, cancellationToken);
          }
          catch (Exception ex) when (ex is not OperationCanceledException)
          {
            _logger.LogError(ex, "Failed to sync exchange rates; retrying in {RetryDelay}.", _lockRetryDelay);
            await ScheduleRetryAsync(lease, cancellationToken);
          }
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

  private async Task WarmCacheAndSignalStartupAsync(CancellationToken cancellationToken)
  {
    using var scope = _serviceProvider.CreateScope();
    var rates = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
    var savedRates = await rates.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);
    var stateRepository = scope.ServiceProvider.GetRequiredService<IRepository<ExchangeRateRefreshState>>();
    var state = await stateRepository.GetByIdAsync(ExchangeRateRefreshState.SingletonId, noTracking: true,
      cancellationToken: cancellationToken)
      ?? throw new InvalidOperationException("Exchange-rate refresh state is missing. Apply database migrations before starting the service.");

    try
    {
      await _exchangeRateCacheManager.CacheAllRatesAsync(savedRates, cancellationToken);
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      _logger.LogWarning(ex, "Could not warm exchange rate cache; database lookup remains available");
    }

    _signal.SignalFirstRunCompleted();
  }

  private async Task<DateTimeOffset> GetNextAttemptAtAsync(CancellationToken cancellationToken)
  {
    using var scope = _serviceProvider.CreateScope();
    var stateRepository = scope.ServiceProvider.GetRequiredService<IRepository<ExchangeRateRefreshState>>();
    var state = await stateRepository.GetByIdAsync(ExchangeRateRefreshState.SingletonId, noTracking: true,
      cancellationToken: cancellationToken)
      ?? throw new InvalidOperationException("Exchange-rate refresh state is missing. Apply database migrations before starting the service.");

    if (state.NextAttemptAt.HasValue)
    {
      return state.NextAttemptAt.Value;
    }

    if (state.LastSuccessfulRefreshAt.HasValue)
    {
      return state.LastSuccessfulRefreshAt.Value.Add(_refreshInterval);
    }

    var repository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
    var savedRates = await repository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);
    return savedRates.Count == 0
      ? DateTimeOffset.UtcNow
      : savedRates.Max(rate => rate.ValidFrom).Add(_refreshInterval);
  }

  private async Task InitializeRefreshScheduleAsync(CancellationToken cancellationToken)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      using (var checkScope = _serviceProvider.CreateScope())
      {
        var stateRepository = checkScope.ServiceProvider.GetRequiredService<IRepository<ExchangeRateRefreshState>>();
        var state = await stateRepository.GetByIdAsync(ExchangeRateRefreshState.SingletonId, noTracking: true,
          cancellationToken: cancellationToken)
          ?? throw new InvalidOperationException("Exchange-rate refresh state is missing. Apply database migrations before starting the service.");
        var exchangeRateRepository = checkScope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
        var savedRates = await exchangeRateRepository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);
        var retryEmptyDatabaseOnStartup = savedRates.Count == 0
          && state.NextAttemptAt.HasValue
          && state.NextAttemptAt.Value > DateTimeOffset.UtcNow;

        if (state.Created != ExchangeRateRefreshState.MigrationSeedTimestamp
          && state.NextAttemptAt.HasValue
          && (state.LastSuccessfulRefreshAt.HasValue || savedRates.Count == 0)
          && !retryEmptyDatabaseOnStartup)
        {
          return;
        }
      }

      IExchangeRateRefreshLease? lease;
      try
      {
        lease = await _refreshLock.TryAcquireAsync(_lockLeaseDuration, cancellationToken);
      }
      catch (Exception ex) when (ex is not OperationCanceledException)
      {
        _logger.LogError(ex, "Could not acquire the exchange-rate refresh lease while initializing its schedule; retrying in {RetryDelay}", _lockRetryDelay);
        await Task.Delay(_lockRetryDelay, cancellationToken);
        continue;
      }

      if (lease is null)
      {
        await Task.Delay(_lockRetryDelay, cancellationToken);
        continue;
      }

      await using (lease)
      {
        lease.ThrowIfLost();
        using var scope = _serviceProvider.CreateScope();
        var stateRepository = scope.ServiceProvider.GetRequiredService<IRepository<ExchangeRateRefreshState>>();
        var state = await stateRepository.GetByIdAsync(ExchangeRateRefreshState.SingletonId, noTracking: false,
          cancellationToken: cancellationToken)
          ?? throw new InvalidOperationException("Exchange-rate refresh state is missing. Apply database migrations before starting the service.");
        var exchangeRateRepository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
        var savedRates = await exchangeRateRepository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);
        var saveChanges = false;
        var lastSuccessfulRefreshAt = state.LastSuccessfulRefreshAt;
        var now = DateTimeOffset.UtcNow;

        if (state.Created == ExchangeRateRefreshState.MigrationSeedTimestamp)
        {
          state.Created = DateTimeOffset.UtcNow;
          saveChanges = true;
        }

        if (!state.LastSuccessfulRefreshAt.HasValue && savedRates.Count > 0)
        {
          state.LastSuccessfulRefreshAt = savedRates.Max(rate => rate.Created);
          saveChanges = true;
        }

        if (!state.NextAttemptAt.HasValue)
        {
          var scheduleAnchor = lastSuccessfulRefreshAt
            ?? (savedRates.Count > 0 ? savedRates.Max(rate => rate.ValidFrom) : null);
          state.NextAttemptAt = scheduleAnchor.HasValue
            ? scheduleAnchor.Value.Add(_refreshInterval)
            : now;
          saveChanges = true;
        }

        if (savedRates.Count == 0 && state.NextAttemptAt > now)
        {
          state.NextAttemptAt = now;
          saveChanges = true;
        }

        if (saveChanges)
        {
          lease.ThrowIfLost();
          var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
          await unitOfWork.SaveChangesAsync(cancellationToken);
        }
      }

      return;
    }
  }

  private async Task RefreshIfDueAsync(IExchangeRateRefreshLease lease, CancellationToken cancellationToken)
  {
    using var leaseCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, lease.LostToken);
    var refreshCancellationToken = leaseCancellation.Token;
    using var scope = _serviceProvider.CreateScope();
    var stateRepository = scope.ServiceProvider.GetRequiredService<IRepository<ExchangeRateRefreshState>>();
    var state = await stateRepository.GetByIdAsync(ExchangeRateRefreshState.SingletonId, noTracking: false,
      cancellationToken: refreshCancellationToken)
      ?? throw new InvalidOperationException("Exchange-rate refresh state is missing. Apply database migrations before starting the service.");

    var nextAttemptAt = await GetNextAttemptAtAsync(refreshCancellationToken);
    if (nextAttemptAt > DateTimeOffset.UtcNow)
    {
      return;
    }

    var exchangeRateRepository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var exchangeRateClient = scope.ServiceProvider.GetRequiredService<IExchangeRateClient>();

    var rates = await exchangeRateClient.GetExchangeRatesAsync();
    if (!rates.IsSuccess || rates.Data is null || rates.Data.Count == 0)
    {
      throw new InvalidDataException($"Exchange rate provider returned no usable rates: {rates.ApplicationError?.Message}");
    }

    lease.ThrowIfLost();
    var actualRates = await exchangeRateRepository.GetActualExchangeRatesAsync(refreshCancellationToken);
    foreach (var rate in actualRates)
    {
      rate.ExpireExchangeRate();
    }

    var refreshedAt = DateTimeOffset.UtcNow;
    state.LastSuccessfulRefreshAt = refreshedAt;
    state.NextAttemptAt = refreshedAt.Add(_refreshInterval);
    await exchangeRateRepository.BatchCreateExchangeRatesAsync(rates.Data, refreshCancellationToken);
    lease.ThrowIfLost();
    await unitOfWork.SaveChangesAsync(refreshCancellationToken);
    _logger.LogInformation("Exchange rates updated successfully. Next refresh is due at {NextAttemptAt}", state.NextAttemptAt);

    try
    {
      var allRates = await exchangeRateRepository.GetAllAsync(noTracking: true, cancellationToken: refreshCancellationToken);
      await _exchangeRateCacheManager.CacheAllRatesAsync(allRates, refreshCancellationToken);
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      _logger.LogWarning(ex, "Saved exchange rates but could not refresh cache; database lookup remains available");
    }
  }

  private async Task ScheduleRetryAsync(IExchangeRateRefreshLease lease, CancellationToken cancellationToken)
  {
    try
    {
      lease.ThrowIfLost();
      using var scope = _serviceProvider.CreateScope();
      var stateRepository = scope.ServiceProvider.GetRequiredService<IRepository<ExchangeRateRefreshState>>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var state = await stateRepository.GetByIdAsync(ExchangeRateRefreshState.SingletonId, noTracking: false,
        cancellationToken: cancellationToken)
        ?? throw new InvalidOperationException("Exchange-rate refresh state is missing. Apply database migrations before starting the service.");
      state.NextAttemptAt = DateTimeOffset.UtcNow.Add(_lockRetryDelay);
      await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
      _logger.LogWarning("Skipped persisting an exchange-rate retry schedule after losing the refresh lease.");
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      _logger.LogError(ex, "Could not persist the exchange-rate retry schedule");
    }
  }
}
