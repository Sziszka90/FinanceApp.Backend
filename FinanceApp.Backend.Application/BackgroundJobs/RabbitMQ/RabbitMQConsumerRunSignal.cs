namespace FinanceApp.Backend.Application.BackgroundJobs.RabbitMQ;

public class RabbitMQConsumerRunSignal
{
  private readonly TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
  private volatile bool _hasRun = false;

  public Task WaitForFirstRunAsync() => _tcs.Task;

  public void SignalFirstRunCompleted()
  {
    _hasRun = true;
    _tcs.TrySetResult(true);
  }

  public bool HasRun => _hasRun;
}
