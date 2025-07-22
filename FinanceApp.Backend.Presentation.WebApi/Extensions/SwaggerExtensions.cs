using Microsoft.OpenApi.Models;

namespace FinanceApp.Backend.Presentation.WebApi.Extensions;

public static class SwaggerExtensions
{
  public static void AddSwagger(this WebApplicationBuilder builder)
  {
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
                                   {
                                     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Finance Application", Version = "v1" });

                                     // Add JWT Bearer Authentication to Swagger
                                     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                                     {
                                       In = ParameterLocation.Header,
                                       Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                                       Name = "Authorization",
                                       Type = SecuritySchemeType.ApiKey
                                     });

                                     // Add security requirement to use the Bearer token for all Swagger operations
                                     c.AddSecurityRequirement(new OpenApiSecurityRequirement
                                     {
                                       {
                                         new OpenApiSecurityScheme
                                         {
                                           Reference = new OpenApiReference
                                           {
                                             Type = ReferenceType.SecurityScheme,
                                             Id = "Bearer"
                                           }
                                         },
                                         new string[] { }
                                       }
                                     });
                                   });
  }

  public static void UseSwaggerConfiguration(this IApplicationBuilder builder)
  {
    builder.UseSwagger();
    builder.UseSwaggerUI();
  }
}
