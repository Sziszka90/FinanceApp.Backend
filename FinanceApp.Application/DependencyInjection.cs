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

namespace FinanceApp.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
  {
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

  private static IServiceCollection AddHttpClient(this IServiceCollection services)
  {
    services.AddHttpClient<IExchangeRateHttpClient, ExchangeRateHttpClient>();
    return services;
  }
}
