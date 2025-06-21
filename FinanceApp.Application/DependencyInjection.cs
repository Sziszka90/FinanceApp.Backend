using System.Reflection;
using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Behaviors;
using FinanceApp.Application.Clients;
using FinanceApp.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;

namespace FinanceApp.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddLLM();
    services.AddClients();
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
    return services;
  }

  private static IServiceCollection AddClients(this IServiceCollection services)
  {
    services.AddScoped<ILLMClient, LLMClient>();
    services.AddScoped<ISmtpEmailSender, SmtpEmailSender>();
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
