using System.Text;
using System.Text.Json.Serialization;
using FinanceApp.Application.Models;
using FinanceApp.Application.Models.Options;
using FinanceApp.Presentation.WebApi.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FinanceApp.Presentation.WebApi.Extensions;

public static class ApiExtensions
{
  public static WebApplicationBuilder SetupApi(this WebApplicationBuilder builder, IConfiguration configuration)
  {
    var authenticationSection = builder.Configuration.GetSection("AuthenticationSettings");
    var authenticationSettings = authenticationSection.Get<AuthenticationSettings>();

    var exchangeRateSection = builder.Configuration.GetSection("ExchangeRateSettings");

    var smtpSection = builder.Configuration.GetSection("SmtpSettings");
    var smtpSettings = smtpSection.Get<SmtpSettings>();

    builder.Services.Configure<AuthenticationSettings>(authenticationSection);
    builder.Services.Configure<ExchangeRateSettings>(exchangeRateSection);
    builder.Services.Configure<SmtpSettings>(smtpSection);

    builder.Services.AddCors(options =>
                             {
                               options.AddPolicy("AllowAllOrigins", policy =>
                                                                    {
                                                                      policy
                                                                        .AllowAnyHeader()
                                                                        .AllowAnyOrigin()
                                                                        .AllowAnyMethod();
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

    // Configure JWT Authentication
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
}
