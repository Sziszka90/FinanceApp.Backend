using System.Text;
using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.ImportPdf;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.MatchTransactionsCommands;
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

      await DeclareQueuesAsync(cancellationToken);
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

  private async Task DeclareQueuesAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await _declarationRetryPolicy.ExecuteAsync(async () =>
      {
        var requestQueueArguments = new Dictionary<string, object?>
        {
          ["x-dead-letter-exchange"] = string.Empty,
          ["x-dead-letter-routing-key"] = _settings.DeadLetterQueue
        };
        await _channel.QueueDeclareAsync(_settings.RequestQueue, durable: true, exclusive: false, autoDelete: false, arguments: requestQueueArguments);
        await _channel.QueueDeclareAsync(_settings.ResultQueue, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueDeclareAsync(_settings.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueDeclareAsync(_settings.PdfRequestQueue, durable: true, exclusive: false, autoDelete: false,
          arguments: new Dictionary<string, object?>
          {
            ["x-dead-letter-exchange"] = string.Empty,
            ["x-dead-letter-routing-key"] = _settings.PdfDeadLetterQueue
          });
        await _channel.QueueDeclareAsync(_settings.PdfResultQueue, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueDeclareAsync(_settings.PdfDeadLetterQueue, durable: true, exclusive: false, autoDelete: false);

        _logger.LogDebug("Successfully declared request, result, and dead-letter queues");
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to declare queues after all retry attempts");
      throw new RabbitMqException("DECLARE_QUEUES", "Failed to declare RabbitMQ queues after all retry attempts.", ex);
    }
  }

  private async Task SetupConsumersAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await _declarationRetryPolicy.ExecuteAsync(async () =>
      {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) => await HandleMessageAsync(ea);

        await _channel.BasicConsumeAsync(queue: _settings.ResultQueue, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

        var pdfConsumer = new AsyncEventingBasicConsumer(_channel);
        pdfConsumer.ReceivedAsync += async (_, ea) => await HandlePdfResultAsync(ea);
        await _channel.BasicConsumeAsync(queue: _settings.PdfResultQueue, autoAck: false, consumer: pdfConsumer,
          cancellationToken: cancellationToken);

        _logger.LogInformation("Successfully setup consumer for result queue {QueueName}", _settings.ResultQueue);
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

      await mediator.Send(new MatchTransactionsCommand(message));
      await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to process message from {RoutingKey}", ea.RoutingKey);
      await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
    }
  }

  private async Task HandlePdfResultAsync(BasicDeliverEventArgs ea)
  {
    try
    {
      var result = JsonSerializer.Deserialize<PdfImportResultDto>(ea.Body.Span)
        ?? throw new JsonException("PDF result was empty.");
      using var scope = _serviceProvider.CreateScope();
      var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
      await mediator.Send(new CompletePdfImportCommand(result));
      await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to process PDF import result");
      await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
    }
  }

  public async Task PublishToQueueAsync(string queueName, string message, CancellationToken cancellationToken = default)
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

      await _channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, mandatory: true, basicProperties: properties, body: body, cancellationToken);
      _logger.LogInformation("Message published to queue {QueueName}", queueName);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to publish message to queue {QueueName}", queueName);
      throw new RabbitMqException("PUBLISH", $"Failed to publish message to queue {queueName}.", ex);
    }
  }
}
