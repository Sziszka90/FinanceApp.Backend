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
using System.Text.Json;

public class RabbitMqClient : IAsyncDisposable, IRabbitMqClient
{
  private readonly ILogger<IRabbitMqClient> _logger;
  private IMediator _mediator;
  private readonly RabbitMqSettings _rabbitMqSettings;
  private readonly IServiceProvider _serviceProvider;

  private IConnection? _connection;
  private IChannel? _channel;
  private ConnectionFactory _factory;


  public RabbitMqClient(
    ILogger<IRabbitMqClient> logger,
    
    IMediator mediator,
    IOptions<RabbitMqSettings> rabbitMqSettings,
    IServiceProvider serviceProvider)
  {
    _logger = logger;
    _mediator = mediator;
    _rabbitMqSettings = rabbitMqSettings.Value;
    _serviceProvider = serviceProvider;

    _factory = new ConnectionFactory
    {
      HostName = _rabbitMqSettings.HostName,
      UserName = _rabbitMqSettings.UserName,
      Password = _rabbitMqSettings.Password,
      Port = _rabbitMqSettings.Port
    };
  }

  public async Task InitializeAsync()
  {
    _connection = await _factory.CreateConnectionAsync();
    _channel = await _connection.CreateChannelAsync();
  }

  public async Task SubscribeAsync(string queue)
  {
    await _channel!.ExchangeDeclareAsync(_rabbitMqSettings.Exchange, ExchangeType.Topic, durable: true);
    await _channel!.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
    await _channel!.QueueBindAsync(queue, _rabbitMqSettings.Exchange, "financeapp.transactions.*");

    var consumer = new AsyncEventingBasicConsumer(_channel!);
    consumer.ReceivedAsync += async (_, ea) =>
    {
      var body = ea.Body.ToArray();
      var message = JsonSerializer.Deserialize<RabbitMQResponseDto>(body);

      if (message == null)
      {
        _logger.LogError("Received null message from RabbitMQ.");
        await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        return;
      }

      var scope = _serviceProvider.CreateScope();
      _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

      switch (ea.RoutingKey)
      {
        case "financeapp.transactions.matched":
          await _mediator.Send(new LLMProcessorCommand(message));
          await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
          break;
        default:
          _logger.LogWarning("Unknown message type: {MessageType}", ea.BasicProperties.Type);
          await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
          break;
      }
    };
    await _channel!.BasicConsumeAsync(queue, autoAck: false, consumer);
  }

  public async ValueTask DisposeAsync()
  {
    if (_channel != null) await _channel.CloseAsync();
    if (_connection != null) await _connection.CloseAsync();
  }

  public Task PublishAsync(string queueName, string message)
  {
    throw new NotImplementedException();
  }
}
