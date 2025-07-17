namespace FinanceApp.Application.Models.Options;

public class RabbitMqSettings
{
  public required string HostName { get; set; }
  public required string UserName { get; set; }
  public required string Password { get; set; }
  public required int Port { get; set; }
  public required string Exchange { get; set; }
  public required RabbitMqTopics Topics { get; set; }
  public required RabbitMqQueues Queues { get; set; }
}

public class RabbitMqTopics
{
  public required string TransactionsMatched { get; set; }
}

public class RabbitMqQueues
{
  public required string TransactionsMatched { get; set; }
}

