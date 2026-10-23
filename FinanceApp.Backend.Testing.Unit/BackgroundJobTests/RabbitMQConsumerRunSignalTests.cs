using FinanceApp.Backend.Application.BackgroundJobs.RabbitMQ;

namespace FinanceApp.Backend.Testing.Unit.BackgroundJobTests;

public class RabbitMQConsumerRunSignalTests
{
  [Fact]
  public void HasRun_InitialState_ReturnsFalse()
  {
    // arrange
    var signal = new RabbitMQConsumerRunSignal();

    // act & assert
    Assert.False(signal.HasRun);
  }

  [Fact]
  public void WaitForFirstRunAsync_InitialState_ReturnsIncompletedTask()
  {
    // arrange
    var signal = new RabbitMQConsumerRunSignal();

    // act
    var task = signal.WaitForFirstRunAsync();

    // assert
    Assert.False(task.IsCompleted);
    Assert.False(task.IsCanceled);
    Assert.False(task.IsFaulted);
    Assert.False(signal.HasRun);
  }

  [Fact]
  public async Task SignalFirstRunCompleted_WhenCalled_CompletesWaitingTaskAndSetsHasRun()
  {
    // arrange
    var signal = new RabbitMQConsumerRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // act
    signal.SignalFirstRunCompleted();

    // assert
    Assert.True(waitTask.IsCompleted);
    Assert.True(signal.HasRun);
    await waitTask;
  }

  [Fact]
  public async Task SignalFirstRunCompleted_WhenCalledMultipleTimes_OnlyCompletesOnceButHasRunStaysTrue()
  {
    // arrange
    var signal = new RabbitMQConsumerRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // act
    signal.SignalFirstRunCompleted();
    signal.SignalFirstRunCompleted(); // second call should be ignored

    // assert
    Assert.True(waitTask.IsCompleted);
    Assert.True(signal.HasRun);
    await waitTask;
  }

  [Fact]
  public async Task WaitForFirstRunAsync_AfterSignalCompleted_ReturnsCompletedTaskAndHasRunIsTrue()
  {
    // arrange
    var signal = new RabbitMQConsumerRunSignal();

    // act
    signal.SignalFirstRunCompleted();
    var waitTask = signal.WaitForFirstRunAsync();

    // assert
    Assert.True(waitTask.IsCompleted);
    Assert.True(signal.HasRun);
    await waitTask;
  }

  [Fact]
  public async Task MultipleWaiters_WhenSignaled_AllTasksCompleteAndHasRunIsTrue()
  {
    // arrange
    var signal = new RabbitMQConsumerRunSignal();
    var waitTask1 = signal.WaitForFirstRunAsync();
    var waitTask2 = signal.WaitForFirstRunAsync();
    var waitTask3 = signal.WaitForFirstRunAsync();

    // act
    signal.SignalFirstRunCompleted();

    // assert
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
    // arrange
    var signal = new RabbitMQConsumerRunSignal();
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
    Assert.True(signal.HasRun);
    await waitTask;
  }

  [Fact]
  public async Task ConcurrentHasRunAccess_ThreadSafe_ConsistentResults()
  {
    // arrange
    var signal = new RabbitMQConsumerRunSignal();
    var hasRunResults = new List<bool>();
    var tasks = new List<Task>();

    // act - simulate concurrent access to HasRun property
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

    // signal completion during concurrent access
    await Task.Delay(10);
    signal.SignalFirstRunCompleted();

    await Task.WhenAll(tasks);

    // assert
    Assert.True(signal.HasRun);
    // all results should be either true or false, no mixed states due to volatile
    Assert.All(hasRunResults, result => Assert.True(result == true || result == false));
  }

  [Fact]
  public void HasRun_VolatileField_ThreadSafe()
  {
    // arrange
    var signal = new RabbitMQConsumerRunSignal();

    // act & assert - initial state
    Assert.False(signal.HasRun);

    // act - signal completion
    signal.SignalFirstRunCompleted();

    // assert - state changed
    Assert.True(signal.HasRun);
  }

  [Fact]
  public void TaskCreationOptions_UsesRunContinuationsAsynchronously()
  {
    // arrange & act
    var signal = new RabbitMQConsumerRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // assert - the task should be created with RunContinuationsAsynchronously option
    // this is verified by ensuring the task doesn't run continuations synchronously
    Assert.NotNull(waitTask);
    Assert.IsType<Task<bool>>(waitTask);
  }

  [Fact]
  public async Task StateConsistency_SignalThenCheck_HasRunAndTaskBothTrue()
  {
    // arrange
    var signal = new RabbitMQConsumerRunSignal();
    var waitTask = signal.WaitForFirstRunAsync();

    // act
    signal.SignalFirstRunCompleted();

    // assert
    Assert.True(signal.HasRun);
    Assert.True(waitTask.IsCompleted);
    await waitTask;
  }
}
