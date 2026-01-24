using FinanceApp.Backend.Application;
using FinanceApp.Backend.Application.Hubs;
using FinanceApp.Backend.Infrastructure;
using FinanceApp.Backend.Presentation.WebApi.Extensions;
using FinanceApp.Backend.Presentation.WebApi.HealthChecks;
using FinanceApp.Backend.Presentation.WebApi.Middlewares;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Default configuration
builder.Configuration.AddJsonFile("appsettings.json")
       .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
       .AddEnvironmentVariables();

// Add services to the container.
builder.SetupApi(builder.Configuration);
builder.AddOpenTelemetryConfiguration();
builder.AddSwagger();

builder.Services.AddApplication(builder.Configuration);

if (builder.Environment.IsTesting())
{
  builder.Services.AddInfrastructureTesting();
}
else
{
  builder.Services.AddInfrastructure(builder.Configuration);
}

builder.Services.AddLogging(config =>
                            {
                              config.AddSimpleConsole(options =>
                                                      {
                                                        options.IncludeScopes = false;
                                                        options.SingleLine = true;
                                                        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                                                      });
                              config.AddDebug();
                            });


var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseApi(builder.Configuration);
app.UseSwaggerConfiguration();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
  Predicate = check => check.Tags.Contains("liveness")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
  Predicate = check => check.Tags.Contains("readiness")
});

app.MapHealthChecks("/health/startup", new HealthCheckOptions
{
  Predicate = check => check.Tags.Contains("startup")
});

app.MapPost("/api/v{version:apiVersion}/wakeup", async (IServiceProvider services) =>
{
  using var scope = services.CreateScope();
  var serviceWakeup = scope.ServiceProvider.GetRequiredService<ServiceWakeup>();
  var result = await serviceWakeup.WakeupAsync();
  if (result)
  {
    return Results.Ok("Services are awake");
  }
  return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
});

app.MapHub<NotificationHub>("/notificationHub")
   .RequireAuthorization()
   .RequireCors("AllowAllOrigins");

app.Run();

public partial class Program { }
