using System.Text;
using System.Text.Json;
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

public class RabbitMqClient : IRabbitMqClient
{
  private readonly ILogger<IRabbitMqClient> _logger;
  private readonly IServiceProvider _serviceProvider;
  private readonly RabbitMqSettings _settings;
  private readonly IRabbitMqConnectionManager _connectionManager;
  private IChannel _channel => _connectionManager.Channel ?? throw new InvalidOperationException("Channel not initialized");

  public RabbitMqClient(
    ILogger<IRabbitMqClient> logger,
    IServiceProvider serviceProvider,
    IOptions<RabbitMqSettings> options,
    IRabbitMqConnectionManager connectionManager)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;
    _settings = options.Value;
    _connectionManager = connectionManager;
  }

  public async Task SubscribeAllAsync()
  {
    await _connectionManager.InitializeAsync();
    await DeclareExchangesAndQueuesAsync();
    await BindQueuesAsync();
    await SetupConsumersAsync();
  }

  private async Task DeclareExchangesAndQueuesAsync()
  {
    foreach (var exchange in _settings.Exchanges)
    {
      await _channel.ExchangeDeclareAsync(exchange.ExchangeName, exchange.ExchangeType, durable: true);
    }

    foreach (var queueName in _settings.Queues)
    {
      await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
    }
  }
  private async Task BindQueuesAsync()
  {
    foreach (var binding in _settings.Bindings)
    {
      var exchangeName = binding.Exchange;
      var queueName = binding.Queue;
      var routingKey = binding.RoutingKey;

      await _channel.QueueBindAsync(queueName, exchangeName, routingKey);
    }
  }
  private async Task SetupConsumersAsync()
  {
    foreach (var queueName in _settings.Queues)
    {

      var consumer = new AsyncEventingBasicConsumer(_channel);
      consumer.ReceivedAsync += async (_, ea) => await HandleMessageAsync(ea);

      await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
    }
  }

  private async Task HandleMessageAsync(BasicDeliverEventArgs ea)
  {
    var body = ea.Body.ToArray();

    try
    {
      var message = JsonSerializer.Deserialize<RabbitMqPayload>(body);
      if (message == null)
        throw new JsonException("Deserialized message is null");

      using var scope = _serviceProvider.CreateScope();
      var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

      switch (ea.RoutingKey)
      {
        case var rk when rk == _settings.RoutingKeys.Where(x => x.Key == "TransactionsMatched").First().Value.RoutingKeyName:
          await mediator.Send(new LLMProcessorCommand(message));
          break;

        default:
          _logger.LogWarning("Unknown routing key: {RoutingKey}", ea.RoutingKey);
          await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
          return;
      }
      await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to process message. RoutingKey: {RoutingKey}, Body: {Body}", ea.RoutingKey, Encoding.UTF8.GetString(body));
      await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
    }
  }

  public Task PublishAsync(string queueName, string message)
  {
    throw new NotImplementedException();
  }
}
