using System.Text;
using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UploadCsv;
using FinanceApp.Backend.Domain.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinanceApp.Backend.Infrastructure.RabbitMq;

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
    try
    {
      await _connectionManager.InitializeAsync();
      await DeclareExchangesAndQueuesAsync();
      await BindQueuesAsync();
      await SetupConsumersAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to subscribe to RabbitMQ");
      throw new RabbitMqException("SUBSCRIBE_ALL", "Failed to initialize RabbitMQ subscriptions.", ex);
    }
  }

  private async Task DeclareExchangesAndQueuesAsync()
  {
    try
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
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to declare exchanges and queues");
      throw new RabbitMqException("DECLARE_EXCHANGES_QUEUES", "Failed to declare RabbitMQ exchanges and queues.", ex);
    }
  }
  private async Task BindQueuesAsync()
  {
    try
    {
      foreach (var binding in _settings.Bindings)
      {
        var exchangeName = binding.Exchange;
        var queueName = binding.Queue;
        var routingKey = binding.RoutingKey;

        await _channel.QueueBindAsync(queueName, exchangeName, routingKey);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to bind queues");
      throw new RabbitMqException("BIND_QUEUES", "Failed to bind RabbitMQ queues to exchanges.", ex);
    }
  }
  private async Task SetupConsumersAsync()
  {
    try
    {
      foreach (var queueName in _settings.Queues)
      {

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) => await HandleMessageAsync(ea);

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to setup consumers");
      throw new RabbitMqException("SETUP_CONSUMERS", "Failed to setup RabbitMQ consumers.", ex);
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
    catch (RabbitMqException)
    {
      await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to process message. RoutingKey: {RoutingKey}, Body: {Body}", ea.RoutingKey, Encoding.UTF8.GetString(body));
      await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
      throw new RabbitMqException("MESSAGE_HANDLING", null, ea.RoutingKey, "Failed to process RabbitMQ message.", ex);
    }
  }

  public Task PublishAsync(string queueName, string message)
  {
    throw new NotImplementedException();
  }
}
