using System.Text;
using Asp.Versioning;
using FinanceApp.Backend.Domain.Options;
using FinanceApp.Backend.Presentation.WebApi.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FinanceApp.Backend.Presentation.WebApi.Extensions;

public static class ApiExtensions
{
  public static WebApplicationBuilder SetupApi(this WebApplicationBuilder builder, IConfiguration configuration)
  {
    builder.AddConfigurations();

    builder.Services.AddCors(options =>
                             {
                               options.AddPolicy("AllowAllOrigins", policy =>
                                                                    {
                                                                      policy.WithOrigins("http://localhost:4200")
                                                                            .AllowAnyHeader()
                                                                            .AllowAnyMethod()
                                                                            .AllowCredentials();
                                                                    });
                             });

    builder.Services.AddHealthChecks()
        .AddCheck<LivenessCheck>("liveness_check", tags: new[] { "liveness" })
        .AddCheck<ReadinessCheck>("readiness_check", tags: new[] { "readiness" })
        .AddCheck<StartupCheck>("startup_check", tags: new[] { "startup" });

    builder.Services.AddScoped<ServiceWakeup>();

    builder.Services.AddControllers().AddNewtonsoftJson(options =>
    {
      options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
      {
        NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()
      };
      options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
      options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

    builder.Services.AddApiVersioning(options =>
    {
      options.AssumeDefaultVersionWhenUnspecified = true;
      options.DefaultApiVersion = new ApiVersion(1, 0);
      options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-Version")
      );
    }).AddApiExplorer(setup =>
    {
      setup.GroupNameFormat = "'v'VVV";
      setup.SubstituteApiVersionInUrl = true;
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

         options.Events = new JwtBearerEvents
         {
           OnMessageReceived = context =>
           {
             var cookieToken = context.Request.Cookies["Token"];
             if (!string.IsNullOrEmpty(cookieToken))
             {
               context.Token = cookieToken;
             }
             else
             {
               var accessToken = context.Request.Query["access_token"];
               var path = context.HttpContext.Request.Path;
               if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
               {
                 context.Token = accessToken;
               }
             }
             return Task.CompletedTask;
           }
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
    var cacheSection = builder.Configuration.GetSection("CacheSettings");

    builder.Services.Configure<AuthenticationSettings>(authenticationSection);
    builder.Services.Configure<ExchangeRateSettings>(exchangeRateSection);
    builder.Services.Configure<SmtpSettings>(smtpSection);
    builder.Services.Configure<RabbitMqSettings>(rabbitMqSection);
    builder.Services.Configure<CacheSettings>(cacheSection);
    builder.Services.Configure<LLMProcessorSettings>(llmProcessorSection);

    return builder;
  }
}
