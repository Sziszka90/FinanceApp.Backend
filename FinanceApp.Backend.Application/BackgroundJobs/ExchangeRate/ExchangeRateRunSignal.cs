namespace FinanceApp.Backend.Application.BackgroundJobs.ExchangeRate;

public class ExchangeRateRunSignal
{
  private readonly TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

  public Task WaitForFirstRunAsync() => _tcs.Task;

  public void SignalFirstRunCompleted() => _tcs.TrySetResult(true);
}
