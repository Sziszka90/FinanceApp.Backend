namespace FinanceApp.Application.Models.Options;

public class RabbitMqSettings
{
  public string HostName { get; set; } = string.Empty;
  public string UserName { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public int Port { get; set; }
  public Dictionary<string, string> Exchanges { get; set; } = new();
  public Dictionary<string, string> Queues { get; set; } = new();
  public Dictionary<string, string> RoutingKeys { get; set; } = new();
  public List<RabbitMqBinding> Bindings { get; set; } = new();
}

public class RabbitMqBinding
{
  public string Exchange { get; set; } = string.Empty;
  public string Queue { get; set; } = string.Empty;
  public string RoutingKey { get; set; } = string.Empty;
}
