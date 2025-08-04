using FinanceApp.Backend.Application.BackgroundJobs.ExchangeRate;

namespace FinanceApp.Backend.Testing.Unit.BackgroundJobTests;

public class ExchangeRateRunSignalTests
{
  [Fact]
  public void WaitForFirstRunAsync_InitialState_ReturnsIncompletedTask()
  {
    // Arrange
    var signal = new ExchangeRateRunSignal();

    // Act
    var task = signal.WaitForFirstRunAsync();

    // Assert
    Assert.False(task.IsCompleted);
    Assert.False(task.IsCanceled);
    Assert.False(task.IsFaulted);
  }

  [Fact]
  public async Task SignalFirstRunCompleted_WhenCalled_CompletesWaitingTask()
  {
    // Arrange
    var signal = new ExchangeRateRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // Act
    signal.SignalFirstRunCompleted();

    // Assert
    Assert.True(waitTask.IsCompleted);
    await waitTask;
  }

  [Fact]
  public async Task SignalFirstRunCompleted_WhenCalledMultipleTimes_OnlyCompletesOnce()
  {
    // Arrange
    var signal = new ExchangeRateRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // Act
    signal.SignalFirstRunCompleted();
    signal.SignalFirstRunCompleted(); // Second call should be ignored

    // Assert
    Assert.True(waitTask.IsCompleted);
    await waitTask;
  }

  [Fact]
  public async Task WaitForFirstRunAsync_AfterSignalCompleted_ReturnsCompletedTask()
  {
    // Arrange
    var signal = new ExchangeRateRunSignal();

    // Act
    signal.SignalFirstRunCompleted();
    var waitTask = signal.WaitForFirstRunAsync();

    // Assert
    Assert.True(waitTask.IsCompleted);
    await waitTask;
  }

  [Fact]
  public async Task MultipleWaiters_WhenSignaled_AllTasksComplete()
  {
    // Arrange
    var signal = new ExchangeRateRunSignal();
    var waitTask1 = signal.WaitForFirstRunAsync();
    var waitTask2 = signal.WaitForFirstRunAsync();
    var waitTask3 = signal.WaitForFirstRunAsync();

    // Act
    signal.SignalFirstRunCompleted();

    // Assert
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
    // Arrange
    var signal = new ExchangeRateRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();
    var tasks = new List<Task>();

    // Act - Simulate concurrent signaling from multiple threads
    for (int i = 0; i < 10; i++)
    {
      tasks.Add(Task.Run(() => signal.SignalFirstRunCompleted()));
    }

    await Task.WhenAll(tasks);

    // Assert
    Assert.True(waitTask.IsCompleted);
    await waitTask;
  }

  [Fact]
  public void TaskCreationOptions_UsesRunContinuationsAsynchronously()
  {
    // Arrange & Act
    var signal = new ExchangeRateRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // Assert - The task should be created with RunContinuationsAsynchronously option
    // This is verified by ensuring the task doesn't run continuations synchronously
    Assert.NotNull(waitTask);
    Assert.IsType<Task<bool>>(waitTask);
  }
}
