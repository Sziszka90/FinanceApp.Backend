using RabbitMQ.Client;

namespace FinanceApp.Backend.Application.Abstraction.Clients;

public interface IRabbitMqConnectionManager
{
  /// <summary>
  /// Initializes the RabbitMQ connection and channel.
  /// </summary>
  Task InitializeAsync();

  /// <summary>
  /// Gets the RabbitMQ channel.
  /// </summary>
  IChannel? Channel { get; }
}
