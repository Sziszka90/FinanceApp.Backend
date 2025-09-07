using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace FinanceApp.Backend.Presentation.WebApi.Extensions;

public static class SwaggerExtensions
{
  public static void AddSwagger(this WebApplicationBuilder builder)
  {
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
                                   {
                                     var serviceProvider = builder.Services.BuildServiceProvider();
                                     var provider = serviceProvider.GetService<IApiVersionDescriptionProvider>();

                                     if (provider != null)
                                     {
                                       foreach (var description in provider.ApiVersionDescriptions)
                                       {
                                         c.SwaggerDoc(description.GroupName, new OpenApiInfo
                                         {
                                           Title = "Finance Application API",
                                           Version = description.ApiVersion.ToString(),
                                           Description = description.IsDeprecated ? " - DEPRECATED" : ""
                                         });
                                       }
                                     }
                                     else
                                     {
                                       c.SwaggerDoc("v1", new OpenApiInfo
                                       {
                                         Title = "Finance Application API",
                                         Version = "v1",
                                         Description = "Finance Application API"
                                       });
                                     }

                                     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                                     {
                                       In = ParameterLocation.Header,
                                       Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                                       Name = "Authorization",
                                       Type = SecuritySchemeType.ApiKey
                                     });

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
    var provider = builder.ApplicationServices.GetService<IApiVersionDescriptionProvider>();

    builder.UseSwagger();
    builder.UseSwaggerUI(c =>
    {
      if (provider != null)
      {
        foreach (var description in provider.ApiVersionDescriptions)
        {
          c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                           $"Finance API {description.GroupName.ToUpperInvariant()}");
        }
      }
      else
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Finance API V1");
      }
    });
  }
}
