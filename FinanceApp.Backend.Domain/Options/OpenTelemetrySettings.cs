namespace FinanceApp.Backend.Domain.Options;

public class OpenTelemetrySettings
{
  public string ServiceName { get; set; } = string.Empty;
  public string ServiceVersion { get; set; } = string.Empty;
  public bool EnableConsoleExporter { get; set; }
  public bool EnableOtlpExporter { get; set; } = true;
  public string OtlpEndpoint { get; set; } = string.Empty;
  public string OtlpProtocol { get; set; } = "Grpc";
  public bool EnableTracing { get; set; } = true;
  public bool EnableMetrics { get; set; } = true;
  public bool EnableLogging { get; set; } = true;
}
