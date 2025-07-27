
namespace FinanceApp.Backend.Application.Abstraction.Clients;

public interface IRabbitMqClient
{
  /// <summary>
  /// Subscribe to a RabbitMQ queue and process messages with the provided handler.
  /// </summary>
  /// <returns>A task that represents the asynchronous operation.</returns>
  /// <exception cref="RabbitMqException">Thrown when the subscription fails</exception>>
  Task SubscribeAllAsync();

  /// <summary>
  /// Publish a message to a RabbitMQ queue.
  /// </summary>
  /// <param name="queueName">The name of the queue.</param>
  /// <param name="message">The message to publish.</param>
  Task PublishAsync(string queueName, string message);
}
