using System.Text;
using System.Text.Json.Serialization;
using FinanceApp.Application.Models.Options;
using FinanceApp.Presentation.WebApi.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FinanceApp.Presentation.WebApi.Extensions;

public static class ApiExtensions
{
  public static WebApplicationBuilder SetupApi(this WebApplicationBuilder builder, IConfiguration configuration)
  {
    builder.AddConfigurations();

    builder.Services.AddCors(options =>
                             {
                               options.AddPolicy("AllowAllOrigins", policy =>
                                                                    {
                                                                      policy
                                                                        .AllowAnyHeader()
                                                                        .WithOrigins("http://localhost:4200")
                                                                        .AllowAnyMethod()
                                                                        .AllowCredentials();
                                                                    });
                             });

    builder.Services.AddHealthChecks()
        .AddCheck<LivenessCheck>("liveness_check", tags: new[] { "liveness" })
        .AddCheck<ReadinessCheck>("readiness_check", tags: new[] { "readiness" })
        .AddCheck<StartupCheck>("startup_check", tags: new[] { "startup" });

    builder.Services.AddControllers()
           .AddJsonOptions(options =>
                           {
                             options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                             options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                           });


    var authenticationSettings = builder.Configuration.GetSection("AuthenticationSettings").Get<AuthenticationSettings>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
                         {
                           options.TokenValidationParameters = new TokenValidationParameters
                           {
                             ValidateIssuer = true,
                             ValidateAudience = true,
                             ValidateLifetime = true,
                             ValidateIssuerSigningKey = true,
                             ValidIssuer = authenticationSettings!.Issuer,
                             ValidAudience = authenticationSettings.Audience,
                             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.SecretKey))
                           };
                         });

    builder.Services.AddAuthorization();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddHttpContextAccessor();

    return builder;
  }

  public static IApplicationBuilder UseApi(this IApplicationBuilder app, IConfiguration configuration)
  {
    app.UseRouting();

    app.UseCors("AllowAllOrigins");

    app.UseAuthorization();

    app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

    return app;
  }

  private static WebApplicationBuilder AddConfigurations(this WebApplicationBuilder builder)
  {
    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile("appsettings.Messaging.json", optional: true, reloadOnChange: true)
        .AddJsonFile("appsettings.Database.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    var authenticationSection = builder.Configuration.GetSection("AuthenticationSettings");
    var exchangeRateSection = builder.Configuration.GetSection("ExchangeRateSettings");
    var smtpSection = builder.Configuration.GetSection("SmtpSettings");
    var rabbitMqSection = builder.Configuration.GetSection("RabbitMqSettings");
    var llmProcessorSection = builder.Configuration.GetSection("LLMProcessorSettings");

    builder.Services.Configure<AuthenticationSettings>(authenticationSection);
    builder.Services.Configure<ExchangeRateSettings>(exchangeRateSection);
    builder.Services.Configure<SmtpSettings>(smtpSection);
    builder.Services.Configure<RabbitMqSettings>(rabbitMqSection);
    builder.Services.Configure<LLMProcessorSettings>(llmProcessorSection);

    return builder;
  }
}
