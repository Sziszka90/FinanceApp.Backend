
namespace FinanceApp.Application.Abstraction.Clients;

public interface IRabbitMqClient
{
  /// <summary>
  /// Subscribe to a RabbitMQ queue and process messages with the provided handler.
  /// </summary>
  /// <param name="queueName">The name of the queue to subscribe to.</param>
  Task SubscribeAsync(string queueName);

  /// <summary>
  /// Publish a message to a RabbitMQ queue.
  /// </summary>
  /// <param name="queueName">The name of the queue.</param>
  /// <param name="message">The message to publish.</param>
  Task PublishAsync(string queueName, string message);

  /// <summary>
  /// Initialize the RabbitMQ client.
  /// </summary>
  Task InitializeAsync();
}
