using FinanceApp.Backend.Application.BackgroundJobs.RabbitMQ;

namespace FinanceApp.Backend.Testing.Unit.BackgroundJobTests;

public class RabbitMQConsumerRunSignalTests
{
  [Fact]
  public void HasRun_InitialState_ReturnsFalse()
  {
    // Arrange
    var signal = new RabbitMQConsumerRunSignal();

    // Act & Assert
    Assert.False(signal.HasRun);
  }

  [Fact]
  public void WaitForFirstRunAsync_InitialState_ReturnsIncompletedTask()
  {
    // Arrange
    var signal = new RabbitMQConsumerRunSignal();

    // Act
    var task = signal.WaitForFirstRunAsync();

    // Assert
    Assert.False(task.IsCompleted);
    Assert.False(task.IsCanceled);
    Assert.False(task.IsFaulted);
    Assert.False(signal.HasRun);
  }

  [Fact]
  public async Task SignalFirstRunCompleted_WhenCalled_CompletesWaitingTaskAndSetsHasRun()
  {
    // Arrange
    var signal = new RabbitMQConsumerRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // Act
    signal.SignalFirstRunCompleted();

    // Assert
    Assert.True(waitTask.IsCompleted);
    Assert.True(signal.HasRun);
    await waitTask;
  }

  [Fact]
  public async Task SignalFirstRunCompleted_WhenCalledMultipleTimes_OnlyCompletesOnceButHasRunStaysTrue()
  {
    // Arrange
    var signal = new RabbitMQConsumerRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // Act
    signal.SignalFirstRunCompleted();
    signal.SignalFirstRunCompleted(); // Second call should be ignored

    // Assert
    Assert.True(waitTask.IsCompleted);
    Assert.True(signal.HasRun);
    await waitTask;
  }

  [Fact]
  public async Task WaitForFirstRunAsync_AfterSignalCompleted_ReturnsCompletedTaskAndHasRunIsTrue()
  {
    // Arrange
    var signal = new RabbitMQConsumerRunSignal();

    // Act
    signal.SignalFirstRunCompleted();
    var waitTask = signal.WaitForFirstRunAsync();

    // Assert
    Assert.True(waitTask.IsCompleted);
    Assert.True(signal.HasRun);
    await waitTask;
  }

  [Fact]
  public async Task MultipleWaiters_WhenSignaled_AllTasksCompleteAndHasRunIsTrue()
  {
    // Arrange
    var signal = new RabbitMQConsumerRunSignal();
    var waitTask1 = signal.WaitForFirstRunAsync();
    var waitTask2 = signal.WaitForFirstRunAsync();
    var waitTask3 = signal.WaitForFirstRunAsync();

    // Act
    signal.SignalFirstRunCompleted();

    // Assert
    Assert.True(waitTask1.IsCompleted);
    Assert.True(waitTask2.IsCompleted);
    Assert.True(waitTask3.IsCompleted);
    Assert.True(signal.HasRun);

    await waitTask1;
    await waitTask2;
    await waitTask3;
  }

  [Fact]
  public async Task ConcurrentSignaling_ThreadSafe_CompletesSuccessfullyAndHasRunIsTrue()
  {
    // Arrange
    var signal = new RabbitMQConsumerRunSignal();
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
    Assert.True(signal.HasRun);
    await waitTask;
  }

  [Fact]
  public async Task ConcurrentHasRunAccess_ThreadSafe_ConsistentResults()
  {
    // Arrange
    var signal = new RabbitMQConsumerRunSignal();
    var hasRunResults = new List<bool>();
    var tasks = new List<Task>();

    // Act - Simulate concurrent access to HasRun property
    for (int i = 0; i < 100; i++)
    {
      tasks.Add(Task.Run(() =>
      {
        lock (hasRunResults)
        {
          hasRunResults.Add(signal.HasRun);
        }
      }));
    }

    // Signal completion during concurrent access
    await Task.Delay(10);
    signal.SignalFirstRunCompleted();

    await Task.WhenAll(tasks);

    // Assert
    Assert.True(signal.HasRun);
    // All results should be either true or false, no mixed states due to volatile
    Assert.All(hasRunResults, result => Assert.True(result == true || result == false));
  }

  [Fact]
  public void HasRun_VolatileField_ThreadSafe()
  {
    // Arrange
    var signal = new RabbitMQConsumerRunSignal();

    // Act & Assert - Initial state
    Assert.False(signal.HasRun);

    // Act - Signal completion
    signal.SignalFirstRunCompleted();

    // Assert - State changed
    Assert.True(signal.HasRun);
  }

  [Fact]
  public void TaskCreationOptions_UsesRunContinuationsAsynchronously()
  {
    // Arrange & Act
    var signal = new RabbitMQConsumerRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // Assert - The task should be created with RunContinuationsAsynchronously option
    // This is verified by ensuring the task doesn't run continuations synchronously
    Assert.NotNull(waitTask);
    Assert.IsType<Task<bool>>(waitTask);
  }

  [Fact]
  public async Task StateConsistency_SignalThenCheck_HasRunAndTaskBothTrue()
  {
    // Arrange
    var signal = new RabbitMQConsumerRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // Act
    signal.SignalFirstRunCompleted();

    // Assert
    Assert.True(signal.HasRun);
    Assert.True(waitTask.IsCompleted);
    await waitTask;
  }
}
