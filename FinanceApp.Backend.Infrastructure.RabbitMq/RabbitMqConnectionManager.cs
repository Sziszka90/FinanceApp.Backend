using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinanceApp.Backend.Infrastructure.RabbitMq;

public class RabbitMqConnectionManager : IRabbitMqConnectionManager, IAsyncDisposable
{
  private readonly ILogger<RabbitMqConnectionManager> _logger;
  private readonly RabbitMqSettings _settings;
  private readonly ConnectionFactory _factory;
  private readonly IAsyncPolicy _retryPolicy;
  private IConnection? _connection;
  private IChannel? _channel;
  public IChannel? Channel => _channel ?? throw new InvalidOperationException("Channel not initialized");
  public bool IsConnected => _connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen;

  public RabbitMqConnectionManager(
    ILogger<RabbitMqConnectionManager> logger,
    IOptions<RabbitMqSettings> settings
  )
  {
    _settings = settings.Value;
    _logger = logger;

    _factory = new ConnectionFactory
    {
      HostName = _settings.HostName,
      UserName = _settings.UserName,
      Password = _settings.Password,
      Port = _settings.Port,
    };

    _retryPolicy = Policy
      .Handle<Exception>()
      .WaitAndRetryAsync(
        retryCount: 5,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), 30)),
        onRetry: (exception, timespan, retryCount, context) =>
        {
          _logger.LogWarning(exception,
            "Failed to initialize RabbitMQ connection to {HostName}:{Port} on attempt {Attempt}/5. Retrying in {Delay} seconds...",
            _settings.HostName, _settings.Port, retryCount, timespan.TotalSeconds);
        });
  }

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await _retryPolicy.ExecuteAsync(async () =>
      {
        _connection = await _factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        _connection.ConnectionShutdownAsync += OnConnectionShutdown;

        _logger.LogInformation("Successfully initialized RabbitMQ connection to {HostName}:{Port}",
        _settings.HostName, _settings.Port);
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to initialize RabbitMQ connection to {HostName}:{Port} after all retry attempts",
        _settings.HostName, _settings.Port);
      throw new RabbitMqException("CONNECTION_INITIALIZE",
        $"Failed to initialize RabbitMQ connection to {_settings.HostName}:{_settings.Port} after all retry attempts.", ex);
    }
  }

  private async Task OnConnectionShutdown(object sender, ShutdownEventArgs e)
  {
    try
    {
      _logger.LogWarning("RabbitMQ connection shutdown: {Reason}", e.ReplyText);
      await Reconnect();
    }
    catch (RabbitMqException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error during connection shutdown handling: {Reason}", e.ReplyText);
      throw new RabbitMqException("CONNECTION_SHUTDOWN_HANDLER", "Unexpected error during connection shutdown handling.", ex);
    }
  }

  private async Task Reconnect(CancellationToken cancellationToken = default)
  {
    try
    {
      _channel?.Dispose();
      _connection?.Dispose();
      _logger.LogDebug("Successfully disposed old RabbitMQ connection/channel during reconnect.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to dispose channel/connection during reconnect. This may indicate a corrupted connection state.");
      throw new RabbitMqException("CONNECTION_CLEANUP", "Failed to cleanup existing connection during reconnect. Connection state may be corrupted.", ex);
    }

    try
    {
      await Task.Delay(TimeSpan.FromSeconds(2));
      await InitializeAsync(cancellationToken);
      _logger.LogInformation("Successfully reconnected to RabbitMQ");
    }
    catch (RabbitMqException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to reconnect to RabbitMQ after cleanup");
      throw new RabbitMqException("CONNECTION_RECONNECT", "Failed to reconnect to RabbitMQ after connection loss.", ex);
    }
  }

  public async ValueTask DisposeAsync()
  {
    try
    {
      if (_channel != null)
      {
        await _channel.CloseAsync();
      }

      if (_connection != null)
      {
        await _connection.CloseAsync();
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during RabbitMQ connection disposal");
    }
  }
}
