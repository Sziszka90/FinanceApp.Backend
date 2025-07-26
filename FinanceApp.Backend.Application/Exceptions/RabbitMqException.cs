namespace FinanceApp.Backend.Application.Exceptions;

public class RabbitMqException : Exception
{
  public string Operation { get; }
  public string? QueueName { get; }
  public string? RoutingKey { get; }
  public string? ExchangeName { get; }

  public RabbitMqException(string operation, string? queueName, string? routingKey, string message)
    : base(message)
  {
    Operation = operation;
    QueueName = queueName;
    RoutingKey = routingKey;
    ExchangeName = null;
  }

  public RabbitMqException(string operation, string? queueName, string? routingKey, string message, Exception innerException)
    : base(message, innerException)
  {
    Operation = operation;
    QueueName = queueName;
    RoutingKey = routingKey;
    ExchangeName = null;
  }

  public RabbitMqException(string operation, string? queueName, string? routingKey, string? exchangeName, string message)
    : base(message)
  {
    Operation = operation;
    QueueName = queueName;
    RoutingKey = routingKey;
    ExchangeName = exchangeName;
  }

  public RabbitMqException(string operation, string? queueName, string? routingKey, string? exchangeName, string message, Exception innerException)
    : base(message, innerException)
  {
    Operation = operation;
    QueueName = queueName;
    RoutingKey = routingKey;
    ExchangeName = exchangeName;
  }

  public RabbitMqException(string operation, string message)
    : base(message)
  {
    Operation = operation;
    QueueName = null;
    RoutingKey = null;
    ExchangeName = null;
  }

  public RabbitMqException(string operation, string message, Exception innerException)
    : base(message, innerException)
  {
    Operation = operation;
    QueueName = null;
    RoutingKey = null;
    ExchangeName = null;
  }
}
