using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using FinanceApp.Backend.Domain.Options;
using OpenTelemetry.Logs;

namespace FinanceApp.Backend.Presentation.WebApi.Extensions;

public static class OpenTelemetryExtensions
{
  public static WebApplicationBuilder AddOpenTelemetryConfiguration(this WebApplicationBuilder builder)
  {
    var configSection = builder.Configuration.GetSection("OpenTelemetrySettings");
    builder.Services.Configure<OpenTelemetrySettings>(configSection);
    var openTelemetrySettings = new OpenTelemetrySettings();
    configSection.Bind(openTelemetrySettings);

    var serviceName = openTelemetrySettings.ServiceName;
    var serviceVersion = openTelemetrySettings.ServiceVersion;
    var enableConsoleExporter = openTelemetrySettings.EnableConsoleExporter;
    var enableOtlpExporter = openTelemetrySettings.EnableOtlpExporter;
    var otlpEndpoint = openTelemetrySettings.OtlpEndpoint;

    var enableTracing = openTelemetrySettings.EnableTracing;
    var enableMetrics = openTelemetrySettings.EnableMetrics;
    var enableLogging = openTelemetrySettings.EnableLogging;

    var resourceBuilder = ResourceBuilder.CreateDefault()
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddTelemetrySdk()
        .AddEnvironmentVariableDetector();

    var otelBuilder = builder.Services.AddOpenTelemetry();

    if (enableTracing)
    {
      otelBuilder.WithTracing(tracing =>
          {
            tracing
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation(options =>
                {
                  options.RecordException = true;
                  options.EnrichWithHttpRequest = (activity, httpRequest) =>
                  {
                    activity.SetTag("http.request.user_agent", httpRequest.Headers.UserAgent.ToString());
                  };
                  options.EnrichWithHttpResponse = (activity, httpResponse) =>
                  {
                    activity.SetTag("http.response.status_code", httpResponse.StatusCode);
                  };
                })
                .AddHttpClientInstrumentation(options =>
                {
                  options.RecordException = true;
                  options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                  {
                    activity.SetTag("http.request.method", httpRequestMessage.Method.ToString());
                  };
                  options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                  {
                    activity.SetTag("http.response.status_code", (int)httpResponseMessage.StatusCode);
                  };
                })
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                  options.SetDbStatementForText = true;
                  options.SetDbStatementForStoredProcedure = true;
                  options.EnrichWithIDbCommand = (activity, command) =>
                  {
                    activity.SetTag("db.command.timeout", command.CommandTimeout);
                  };
                })
                .AddSource(serviceName);

            if (enableConsoleExporter)
            {
              tracing.AddConsoleExporter();
            }

            if (enableOtlpExporter)
            {
              tracing.AddOtlpExporter(options =>
              {
                options.Endpoint = new Uri(otlpEndpoint);
                options.Protocol = openTelemetrySettings.OtlpProtocol?.Equals("HttpProtobuf", StringComparison.OrdinalIgnoreCase) == true
                    ? OtlpExportProtocol.HttpProtobuf
                    : OtlpExportProtocol.Grpc;
              });

              Console.WriteLine($"[OpenTelemetry] OTLP Trace Exporter configured: {otlpEndpoint}");
            }
          });
    }

    if (enableMetrics)
    {
      otelBuilder.WithMetrics(metrics =>
          {
            metrics
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(serviceName);

            if (enableConsoleExporter)
            {
              metrics.AddConsoleExporter();
            }

            if (enableOtlpExporter)
            {
              metrics.AddOtlpExporter(options =>
              {
                options.Endpoint = new Uri(otlpEndpoint);
                options.Protocol = openTelemetrySettings.OtlpProtocol?.Equals("HttpProtobuf", StringComparison.OrdinalIgnoreCase) == true
                    ? OtlpExportProtocol.HttpProtobuf
                    : OtlpExportProtocol.Grpc;

                options.ExportProcessorType = ExportProcessorType.Simple;
              });
            }
          });
    }

    if (enableLogging)
    {
      builder.Logging.AddOpenTelemetry(logging =>
      {
        logging.SetResourceBuilder(resourceBuilder);
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.ParseStateValues = true;

        if (enableOtlpExporter)
        {
          logging.AddOtlpExporter(options =>
          {
            options.Endpoint = new Uri(otlpEndpoint);
            options.Protocol = openTelemetrySettings.OtlpProtocol?.Equals("HttpProtobuf", StringComparison.OrdinalIgnoreCase) == true
                ? OtlpExportProtocol.HttpProtobuf
                : OtlpExportProtocol.Grpc;
          });

          Console.WriteLine($"[OpenTelemetry] OTLP Log Exporter configured: {otlpEndpoint}");
        }

        if (enableConsoleExporter)
        {
          logging.AddConsoleExporter();
        }
      });
    }

    return builder;
  }
}
