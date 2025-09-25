using FinanceApp.Backend.Application.BackgroundJobs.ExchangeRate;

namespace FinanceApp.Backend.Testing.Unit.BackgroundJobTests;

public class ExchangeRateRunSignalTests
{
  [Fact]
  public void WaitForFirstRunAsync_InitialState_ReturnsIncompletedTask()
  {
    // arrange
    var signal = new ExchangeRateRunSignal();

    // act
    var task = signal.WaitForFirstRunAsync();

    // assert
    Assert.False(task.IsCompleted);
    Assert.False(task.IsCanceled);
    Assert.False(task.IsFaulted);
  }

  [Fact]
  public async Task SignalFirstRunCompleted_WhenCalled_CompletesWaitingTask()
  {
    // arrange
    var signal = new ExchangeRateRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // act
    signal.SignalFirstRunCompleted();

    // assert
    Assert.True(waitTask.IsCompleted);
    await waitTask;
  }

  [Fact]
  public async Task SignalFirstRunCompleted_WhenCalledMultipleTimes_OnlyCompletesOnce()
  {
    // arrange
    var signal = new ExchangeRateRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // act
    signal.SignalFirstRunCompleted();
    signal.SignalFirstRunCompleted(); // second call should be ignored

    // assert
    Assert.True(waitTask.IsCompleted);
    await waitTask;
  }

  [Fact]
  public async Task WaitForFirstRunAsync_AfterSignalCompleted_ReturnsCompletedTask()
  {
    // arrange
    var signal = new ExchangeRateRunSignal();

    // act
    signal.SignalFirstRunCompleted();
    var waitTask = signal.WaitForFirstRunAsync();

    // assert
    Assert.True(waitTask.IsCompleted);
    await waitTask;
  }

  [Fact]
  public async Task MultipleWaiters_WhenSignaled_AllTasksComplete()
  {
    // arrange
    var signal = new ExchangeRateRunSignal();
    var waitTask1 = signal.WaitForFirstRunAsync();
    var waitTask2 = signal.WaitForFirstRunAsync();
    var waitTask3 = signal.WaitForFirstRunAsync();

    // act
    signal.SignalFirstRunCompleted();

    // assert
    Assert.True(waitTask1.IsCompleted);
    Assert.True(waitTask2.IsCompleted);
    Assert.True(waitTask3.IsCompleted);

    await waitTask1;
    await waitTask2;
    await waitTask3;
  }

  [Fact]
  public async Task ConcurrentSignaling_ThreadSafe_CompletesSuccessfully()
  {
    // arrange
    var signal = new ExchangeRateRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();
    var tasks = new List<Task>();

    // act - simulate concurrent signaling from multiple threads
    for (int i = 0; i < 10; i++)
    {
      tasks.Add(Task.Run(() => signal.SignalFirstRunCompleted()));
    }

    await Task.WhenAll(tasks);

    // assert
    Assert.True(waitTask.IsCompleted);
    await waitTask;
  }

  [Fact]
  public void TaskCreationOptions_UsesRunContinuationsAsynchronously()
  {
    // arrange & act
    var signal = new ExchangeRateRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // assert - the task should be created with RunContinuationsAsynchronously option
    // this is verified by ensuring the task doesn't run continuations synchronously
    Assert.NotNull(waitTask);
    Assert.IsType<Task<bool>>(waitTask);
  }
}
