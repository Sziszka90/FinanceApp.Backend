using FinanceApp.Backend.Application.Abstraction.Services;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace FinanceApp.Backend.Infrastructure.Cache;

public sealed class RedisExchangeRateRefreshLock : IExchangeRateRefreshLock
{
  private static readonly TimeSpan _renewalInterval = TimeSpan.FromSeconds(30);
  private readonly IDatabase _database;
  private readonly string _lockKey;

  public RedisExchangeRateRefreshLock(IConnectionMultiplexer connection, IConfiguration configuration)
  {
    _database = connection.GetDatabase();
    var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Default";
    _lockKey = $"FinanceApp:{environment}:Jobs:ExchangeRateRefresh";
  }

  public async Task<IExchangeRateRefreshLease?> TryAcquireAsync(
    TimeSpan leaseDuration, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();
    var ownerToken = Guid.NewGuid().ToString("N");
    var acquired = await _database.LockTakeAsync(_lockKey, ownerToken, leaseDuration);
    return acquired ? new RedisExchangeRateRefreshLease(_database, _lockKey, ownerToken, leaseDuration, _renewalInterval) : null;
  }
}

internal sealed class RedisExchangeRateRefreshLease : IExchangeRateRefreshLease
{
  private readonly IDatabase _database;
  private readonly RedisKey _key;
  private readonly RedisValue _ownerToken;
  private readonly TimeSpan _leaseDuration;
  private readonly CancellationTokenSource _renewalCancellation = new();
  private readonly CancellationTokenSource _lostCancellation = new();
  private readonly Task _renewalTask;
  private int _disposed;

  public RedisExchangeRateRefreshLease(
    IDatabase database, RedisKey key, RedisValue ownerToken, TimeSpan leaseDuration, TimeSpan renewalInterval)
  {
    _database = database;
    _key = key;
    _ownerToken = ownerToken;
    _leaseDuration = leaseDuration;
    _renewalTask = RenewAsync(renewalInterval);
  }

  public CancellationToken LostToken => _lostCancellation.Token;

  public void ThrowIfLost() => LostToken.ThrowIfCancellationRequested();

  private async Task RenewAsync(TimeSpan renewalInterval)
  {
    using var timer = new PeriodicTimer(renewalInterval);
    try
    {
      while (await timer.WaitForNextTickAsync(_renewalCancellation.Token))
      {
        if (!await _database.LockExtendAsync(_key, _ownerToken, _leaseDuration))
        {
          _lostCancellation.Cancel();
          return;
        }
      }
    }
    catch (OperationCanceledException) when (_renewalCancellation.IsCancellationRequested)
    {
      // Normal lease disposal.
    }
    catch
    {
      // Fail closed: the job must not continue if ownership cannot be confirmed.
      _lostCancellation.Cancel();
    }
  }

  public async ValueTask DisposeAsync()
  {
    if (Interlocked.Exchange(ref _disposed, 1) != 0)
    {
      return;
    }

    await _renewalCancellation.CancelAsync();
    await _renewalTask;
    if (!LostToken.IsCancellationRequested)
    {
      try
      {
        await _database.LockReleaseAsync(_key, _ownerToken);
      }
      catch
      {
        // The lease will expire automatically if Redis is unavailable during release.
      }
    }

    _renewalCancellation.Dispose();
    _lostCancellation.Dispose();
  }
}
