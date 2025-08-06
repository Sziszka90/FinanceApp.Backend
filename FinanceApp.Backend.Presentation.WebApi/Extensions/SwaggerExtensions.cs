using Microsoft.OpenApi.Models;
using Asp.Versioning.ApiExplorer;

namespace FinanceApp.Backend.Presentation.WebApi.Extensions;

public static class SwaggerExtensions
{
  public static void AddSwagger(this WebApplicationBuilder builder)
  {
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
                                   {
                                     // Configure Swagger to generate documentation for each API version
                                     var provider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                                     foreach (var description in provider.ApiVersionDescriptions)
                                     {
                                       c.SwaggerDoc(description.GroupName, new OpenApiInfo
                                       {
                                         Title = "Finance Application API",
                                         Version = description.ApiVersion.ToString(),
                                         Description = description.IsDeprecated ? " - DEPRECATED" : ""
                                       });
                                     }

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
    var provider = builder.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

    builder.UseSwagger();
    builder.UseSwaggerUI(c =>
    {
      foreach (var description in provider.ApiVersionDescriptions)
      {
        c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                         $"Finance API {description.GroupName.ToUpperInvariant()}");
      }
    });
  }
}
