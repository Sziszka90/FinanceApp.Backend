using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Dtos.RabbitMQDtos;
using FinanceApp.Application.Models.Options;
using FinanceApp.Application.TransactionApi.TransactionCommands.UploadCsv;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class RabbitMqClient : IAsyncDisposable, IRabbitMqClient
{
  private readonly ILogger<IRabbitMqClient> _logger;
  private readonly IServiceProvider _serviceProvider;
  private readonly RabbitMqSettings _settings;
  private readonly ConnectionFactory _factory;
  private IConnection? _connection;
  private IChannel? _channel;

  public RabbitMqClient(
    ILogger<IRabbitMqClient> logger,
    IServiceProvider serviceProvider,
    IOptions<RabbitMqSettings> rabbitMqSettings)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;
    _settings = rabbitMqSettings.Value;

    _factory = new ConnectionFactory
    {
      HostName = _settings.HostName,
      UserName = _settings.UserName,
      Password = _settings.Password,
      Port = _settings.Port
    };
  }

  public async Task InitializeAsync()
  {
    _connection = await _factory.CreateConnectionAsync();
    _channel = await _connection.CreateChannelAsync();
  }

  public async Task SubscribeAllAsync()
  {
    if (_channel == null)
      throw new InvalidOperationException("RabbitMQ channel not initialized.");

    await DeclareExchangesAndQueuesAsync();
    await BindQueuesAsync();
    await SetupConsumersAsync();
  }

  private async Task DeclareExchangesAndQueuesAsync()
  {
    foreach (var exchange in _settings.Exchanges.Values)
    {
      await _channel!.ExchangeDeclareAsync(exchange, GetExchangeType(exchange), durable: true);
    }

    foreach (var queue in _settings.Queues.Values)
    {
      await _channel!.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
    }
  }

  private async Task BindQueuesAsync()
  {
    foreach (var binding in _settings.Bindings)
    {
      var exchangeName = _settings.Exchanges[binding.Exchange];
      var queueName = _settings.Queues[binding.Queue];
      var routingKey = binding.RoutingKey;

      await _channel!.QueueBindAsync(queueName, exchangeName, routingKey);
    }
  }

  private async Task SetupConsumersAsync()
  {
    foreach (var queuePair in _settings.Queues)
    {
      string queueKey = queuePair.Key;
      string queueName = queuePair.Value;

      var consumer = new AsyncEventingBasicConsumer(_channel!);
      consumer.ReceivedAsync += async (_, ea) => await HandleMessageAsync(ea);

      await _channel!.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
    }
  }

  private async Task HandleMessageAsync(BasicDeliverEventArgs ea)
  {
    var body = ea.Body.ToArray();

    try
    {
      var message = JsonSerializer.Deserialize<RabbitMQResponseDto>(body);
      if (message == null)
        throw new JsonException("Deserialized message is null");

      using var scope = _serviceProvider.CreateScope();
      var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

      switch (ea.RoutingKey)
      {
        case var rk when rk == _settings.RoutingKeys["TransactionsMatched"]:
          await mediator.Send(new LLMProcessorCommand(message));
          break;

        default:
          _logger.LogWarning("Unknown routing key: {RoutingKey}", ea.RoutingKey);
          await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
          return;
      }
      await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to process message. RoutingKey: {RoutingKey}, Body: {Body}", ea.RoutingKey, Encoding.UTF8.GetString(body));
      await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
    }
  }

  public Task PublishAsync(string queueName, string message)
  {
    throw new NotImplementedException();
  }

  public async ValueTask DisposeAsync()
  {
    if (_channel != null) await _channel.CloseAsync();
    if (_connection != null) await _connection.CloseAsync();
  }

  private string GetExchangeType(string exchangeName)
  {
    return exchangeName.ToLowerInvariant() switch
    {
      var name when name.Contains("topic") => ExchangeType.Topic,
      var name when name.Contains("direct") => ExchangeType.Direct,
      _ => throw new NotSupportedException($"Exchange type for {exchangeName} not supported."),
    };
  }
}

