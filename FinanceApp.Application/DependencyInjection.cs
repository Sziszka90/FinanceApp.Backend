using System.Reflection;
using FinanceApp.Application.Abstraction.HttpClients;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Behaviors;
using FinanceApp.Application.HttpClients;
using FinanceApp.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;

namespace FinanceApp.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddLLM();
    services.AddAutoMapper(config => { config.AddMaps(typeof(DependencyInjection).Assembly); });
    services.AddMediatR(config => config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
    services.AddValidators();
    services.AddBehaviors();
    services.AddHttpClient();
    services.AddServices();
    return services;
  }

  private static IServiceCollection AddValidators(this IServiceCollection services)
  {
    services.AddValidatorsFromAssemblyContaining(typeof(ValidationBehavior<,>));

    ValidatorOptions.Global.DisplayNameResolver = (type, member, expression) => $"{type.Name}.{member?.Name}";

    return services;
  }

  private static IServiceCollection AddBehaviors(this IServiceCollection services)
  {
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    return services;
  }

  private static IServiceCollection AddServices(this IServiceCollection services)
  {
    services.AddScoped<IJwtService, JwtService>();
    services.AddScoped<ILLMClient, LLMClient>();
    return services;
  }

  private static IServiceCollection AddLLM(this IServiceCollection services)
  {
    services.AddScoped(sp =>
    {
        var apiKey = sp.GetRequiredService<IConfiguration>()["LLMClientSettings:ApiKey"];
        ChatClient client = new(model: "gpt-4o", apiKey);
        return client;
    });
    return services;
  }
}
