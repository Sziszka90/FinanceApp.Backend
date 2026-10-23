using RabbitMQ.Client;

namespace FinanceApp.Backend.Application.Abstraction.Clients;

public interface IRabbitMqConnectionManager
{
  /// <summary>
  /// Initializes the RabbitMQ connection and channel.
  /// </summary>
  /// <returns>A task that represents the asynchronous operation.</returns>
  /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  /// <exception cref="RabbitMqException">Thrown when the connection fails</exception>
  Task InitializeAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the RabbitMQ channel.
  /// </summary>
  IChannel? Channel { get; }

  /// <summary>
  /// Checks if the RabbitMQ connection is established and the channel is open.
  /// </summary>
  bool IsConnected { get; }
}
