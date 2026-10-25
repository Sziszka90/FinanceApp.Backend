namespace FinanceApp.Backend.Application.Abstraction.Services;

public interface IExchangeRateRefreshLock
{
  Task<IExchangeRateRefreshLease?> TryAcquireAsync(TimeSpan leaseDuration, CancellationToken cancellationToken = default);
}

public interface IExchangeRateRefreshLease : IAsyncDisposable
{
  CancellationToken LostToken { get; }

  void ThrowIfLost();
}
