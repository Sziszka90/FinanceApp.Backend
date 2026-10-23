namespace FinanceApp.Backend.Domain.Options;

public class RabbitMqSettings
{
  public string HostName { get; set; } = string.Empty;
  public string UserName { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public int Port { get; set; }
  public string RequestQueue { get; set; } = string.Empty;
  public string ResultQueue { get; set; } = string.Empty;
  public string DeadLetterQueue { get; set; } = string.Empty;
  public string PdfRequestQueue { get; set; } = string.Empty;
  public string PdfResultQueue { get; set; } = string.Empty;
  public string PdfDeadLetterQueue { get; set; } = string.Empty;
}
