using RabbitMQ.Client;

namespace FinanceApp.Backend.Application.Abstraction.Clients;

public interface IRabbitMqConnectionManager
{
  /// <summary>
  /// Initializes the RabbitMQ connection and channel.
  /// </summary>
  /// <returns>A task that represents the asynchronous operation.</returns>
  /// <exception cref="RabbitMqException">Thrown when the connection fails</exception>
  Task InitializeAsync();

  /// <summary>
  /// Gets the RabbitMQ channel.
  /// </summary>
  IChannel? Channel { get; }
}
