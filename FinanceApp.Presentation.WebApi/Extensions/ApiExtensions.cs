using System.Text;
using System.Text.Json.Serialization;
using FinanceApp.Application.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FinanceApp.Presentation.WebApi.Extensions;

public static class ApiExtensions
{
  #region Methods

  public static WebApplicationBuilder SetupApi(this WebApplicationBuilder builder, IConfiguration configuration)
  {
    var authenticationSection = builder.Configuration.GetSection("AuthenticationSettings");
    var authenticationSettings = authenticationSection.Get<AuthenticationSettings>();

    var exchangeRateSection = builder.Configuration.GetSection("ExchangeRateSettings");

    builder.Services.Configure<AuthenticationSettings>(authenticationSection);
    builder.Services.Configure<ExchangeRateSettings>(exchangeRateSection);

    builder.Services.AddCors(options =>
                             {
                               options.AddPolicy("AllowAllOrigins", policy =>
                                                                    {
                                                                      policy
                                                                        .AllowAnyHeader()
                                                                        .WithOrigins("https://0.0.0.0",
                                                                                     "app://0.0.0.0",
                                                                                     "https://localhost:4200",
                                                                                     "http://localhost:4200"
                                                                        )
                                                                        .AllowAnyMethod()
                                                                        .AllowCredentials();
                                                                    });
                             });

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

  #endregion
}
