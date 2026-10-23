
namespace FinanceApp.Backend.Application.Abstraction.Clients;

public interface IRabbitMqClient
{
  /// <summary>
  /// Subscribe to a RabbitMQ queue and process messages with the provided handler.
  /// </summary>
  /// <returns>A task that represents the asynchronous operation.</returns>
  /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  /// <exception cref="RabbitMqException">Thrown when the subscription fails</exception>
  Task SubscribeAllAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Publish a message to a RabbitMQ queue.
  /// </summary>
  /// <param name="queueName">The queue to publish the message to.</param>
  /// <param name="message">The message to publish.</param>
  /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  /// <exception cref="RabbitMqException">Thrown when the publish operation fails</exception>
  Task PublishToQueueAsync(string queueName, string message, CancellationToken cancellationToken = default);
}
