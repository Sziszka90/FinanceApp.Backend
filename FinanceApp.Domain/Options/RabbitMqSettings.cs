namespace FinanceApp.Domain.Options;

public class RabbitMqSettings
{
  public string HostName { get; set; } = string.Empty;
  public string UserName { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public int Port { get; set; }
  public List<Exchange> Exchanges { get; set; } = new();
  public List<string> Queues { get; set; } = new();
  public Dictionary<string, RoutingKey> RoutingKeys { get; set; } = new();
  public List<RabbitMqBinding> Bindings { get; set; } = new();
}

public class RabbitMqBinding
{
  public string Exchange { get; set; } = string.Empty;
  public string Queue { get; set; } = string.Empty;
  public string RoutingKey { get; set; } = string.Empty;
}

public class Exchange
{
  public string ExchangeName { get; set; } = string.Empty;
  public string ExchangeType { get; set; } = string.Empty;
}

public class RoutingKey
{
  public string ExchangeName { get; set; } = string.Empty;
  public string RoutingKeyName { get; set; } = string.Empty;
}
