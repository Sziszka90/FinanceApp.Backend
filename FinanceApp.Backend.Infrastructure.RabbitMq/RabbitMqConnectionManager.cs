using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinanceApp.Backend.Infrastructure.RabbitMq;

public class RabbitMqConnectionManager : IRabbitMqConnectionManager, IAsyncDisposable
{
  private readonly ILogger<RabbitMqConnectionManager> _logger;
  private readonly RabbitMqSettings _settings;
  private readonly ConnectionFactory _factory;
  private IConnection? _connection;
  private IChannel? _channel;
  public IChannel? Channel => _channel ?? throw new InvalidOperationException("Channel not initialized");

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
  }

  public async Task InitializeAsync()
  {
    _connection = await _factory.CreateConnectionAsync();
    _channel = await _connection.CreateChannelAsync();
    _connection.ConnectionShutdownAsync += OnConnectionShutdown;
  }

  private async Task OnConnectionShutdown(object sender, ShutdownEventArgs e)
  {
    _logger.LogWarning("RabbitMQ connection shutdown: {Reason}", e.ReplyText);
    await Reconnect();
  }

  private async Task Reconnect()
  {
    try
    {
      _channel?.Dispose();
      _connection?.Dispose();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error disposing channel/connection during reconnect.");
    }

    await Task.Delay(TimeSpan.FromSeconds(2));
    await InitializeAsync();
  }

  public async ValueTask DisposeAsync()
  {
    if (_channel != null) await _channel.CloseAsync();
    if (_connection != null) await _connection.CloseAsync();
  }
}
