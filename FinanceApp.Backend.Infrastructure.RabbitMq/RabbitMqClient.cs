using System.Text;
using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.MatchTransactionsCommands;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UploadCsv;
using FinanceApp.Backend.Domain.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinanceApp.Backend.Infrastructure.RabbitMq;

public class RabbitMqClient : IRabbitMqClient
{
  private readonly ILogger<IRabbitMqClient> _logger;
  private readonly IServiceProvider _serviceProvider;
  private readonly RabbitMqSettings _settings;
  private readonly IRabbitMqConnectionManager _connectionManager;
  private readonly IAsyncPolicy _declarationRetryPolicy;
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

    _declarationRetryPolicy = Policy
      .Handle<Exception>()
      .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), 10)),
        onRetry: (exception, timespan, retryCount, context) =>
        {
          _logger.LogWarning(exception,
            "Failed RabbitMQ declaration operation on attempt {Attempt}/3. Retrying in {Delay} seconds...",
            retryCount, timespan.TotalSeconds);
        });
  }

  public async Task SubscribeAllAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await _connectionManager.InitializeAsync(cancellationToken);

      if (_connectionManager.IsConnected == false)
      {
        _logger.LogError("RabbitMQ connection could not be established. Cannot subscribe to queues.");
        throw new RabbitMqException("SUBSCRIBE_ALL", "RabbitMQ connection could not be established.");
      }

      await DeclareExchangesAndQueuesAsync(cancellationToken);
      await BindQueuesAsync(cancellationToken);
      await SetupConsumersAsync(cancellationToken);

      _logger.LogInformation("Successfully initialized RabbitMQ subscriptions");
    }
    catch (RabbitMqException ex)
    {
      _logger.LogError(ex, "Critical failure: RabbitMQ subscription setup failed. Stopping application.");
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Critical failure: Unexpected error during RabbitMQ subscription setup. Stopping application.");
      throw new RabbitMqException("SUBSCRIBE_ALL", "Failed to initialize RabbitMQ subscriptions.", ex);
    }
  }

  private async Task DeclareExchangesAndQueuesAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await _declarationRetryPolicy.ExecuteAsync(async () =>
      {
        foreach (var exchange in _settings.Exchanges)
        {
          await _channel.ExchangeDeclareAsync(exchange.ExchangeName, exchange.ExchangeType, durable: true);
        }

        foreach (var queueName in _settings.Queues)
        {
          await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);
        }

        _logger.LogDebug("Successfully declared {ExchangeCount} exchanges and {QueueCount} queues",
          _settings.Exchanges.Count, _settings.Queues.Count);
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to declare exchanges and queues after all retry attempts");
      throw new RabbitMqException("DECLARE_EXCHANGES_QUEUES", "Failed to declare RabbitMQ exchanges and queues after all retry attempts.", ex);
    }
  }

  private async Task BindQueuesAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await _declarationRetryPolicy.ExecuteAsync(async () =>
      {
        foreach (var binding in _settings.Bindings)
        {
          var exchangeName = binding.Exchange;
          var queueName = binding.Queue;
          var routingKey = binding.RoutingKey;

          await _channel.QueueBindAsync(queueName, exchangeName, routingKey, cancellationToken: cancellationToken);
        }

        _logger.LogDebug("Successfully bound {BindingCount} queue bindings", _settings.Bindings.Count);
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to bind queues after all retry attempts");
      throw new RabbitMqException("BIND_QUEUES", "Failed to bind RabbitMQ queues to exchanges after all retry attempts.", ex);
    }
  }
  private async Task SetupConsumersAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await _declarationRetryPolicy.ExecuteAsync(async () =>
      {
        foreach (var queueName in _settings.Queues)
        {
          var consumer = new AsyncEventingBasicConsumer(_channel);
          consumer.ReceivedAsync += async (_, ea) => await HandleMessageAsync(ea);

          await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
        }

        _logger.LogInformation("Successfully setup consumers for {QueueCount} queues", _settings.Queues.Count);
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to setup consumers after all retry attempts");
      throw new RabbitMqException("SETUP_CONSUMERS", "Failed to setup RabbitMQ consumers after all retry attempts.", ex);
    }
  }

  private async Task HandleMessageAsync(BasicDeliverEventArgs ea)
  {
    var body = ea.Body.ToArray();

    try
    {
      var settings = new Newtonsoft.Json.JsonSerializerSettings
      {
        ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
        {
          NamingStrategy = new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy()
        }
      };

      var jsonString = Encoding.UTF8.GetString(body);
      var message = Newtonsoft.Json.JsonConvert.DeserializeObject<RabbitMqPayload>(jsonString, settings) ?? throw new JsonException("Deserialized message is null");

      using var scope = _serviceProvider.CreateScope();
      var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

      switch (ea.RoutingKey)
      {
        case var rk when rk == _settings.RoutingKeys.Where(x => x.Key == "TransactionsMatched").First().Value.RoutingKeyName:
          await mediator.Send(new MatchTransactionsCommand(message));
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

  public async Task PublishAsync(string exchangeName, string routingKey, string message, CancellationToken cancellationToken = default)
  {
    if (_connectionManager.IsConnected == false)
    {
      _logger.LogError("RabbitMQ connection is not established. Cannot publish message.");
      throw new RabbitMqException("PUBLISH", "RabbitMQ connection is not established.");
    }

    try
    {
      var body = Encoding.UTF8.GetBytes(message);
      var properties = new BasicProperties();
      properties.Persistent = true;

      await _channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, mandatory: true, basicProperties: properties, body: body, cancellationToken);
      _logger.LogInformation("Message published to exchange {ExchangeName}, routing key {RoutingKey}: {Message}", exchangeName, routingKey, message);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to publish message to exchange {ExchangeName}, routing key {RoutingKey}: {Message}", exchangeName, routingKey, message);
      throw new RabbitMqException("PUBLISH", $"Failed to publish message to exchange {exchangeName}, routing key {routingKey}.", ex);
    }
  }
}
