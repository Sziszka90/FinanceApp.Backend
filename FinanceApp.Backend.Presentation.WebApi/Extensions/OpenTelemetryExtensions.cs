using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FinanceApp.Backend.Presentation.WebApi.Extensions;

public static class OpenTelemetryExtensions
{
  public static WebApplicationBuilder AddOpenTelemetryConfiguration(this WebApplicationBuilder builder)
  {
    var openTelemetryConfig = builder.Configuration.GetSection("OpenTelemetry");
    var serviceName = openTelemetryConfig["ServiceName"] ?? "FinanceApp.Backend";
    var serviceVersion = openTelemetryConfig["ServiceVersion"] ?? "1.0.0";
    var enableConsoleExporter = openTelemetryConfig.GetValue<bool>("EnableConsoleExporter");
    var enableOtlpExporter = openTelemetryConfig.GetValue<bool>("EnableOtlpExporter");
    var otlpEndpoint = openTelemetryConfig["OtlpEndpoint"] ?? "http://localhost:4317";
    var enableTracing = openTelemetryConfig.GetValue<bool>("EnableTracing", true);
    var enableMetrics = openTelemetryConfig.GetValue<bool>("EnableMetrics", true);

    var resourceBuilder = ResourceBuilder.CreateDefault()
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddTelemetrySdk()
        .AddEnvironmentVariableDetector();

    if (enableTracing)
    {
      builder.Services.AddOpenTelemetry()
          .WithTracing(tracing =>
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
              });
            }
          });
    }

    if (enableMetrics)
    {
      builder.Services.AddOpenTelemetry()
          .WithMetrics(metrics =>
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
              });
            }
          });
    }

    return builder;
  }
}
