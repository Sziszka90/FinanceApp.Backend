using System.Reflection;
using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.BackgroundJobs.ExchangeRate;
using FinanceApp.Application.BackgroundJobs.RabbitMQ;
using FinanceApp.Application.Behaviors;
using FinanceApp.Application.Clients;
using FinanceApp.Application.Models.Options;
using FinanceApp.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace FinanceApp.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddClients();
    services.AddAutoMapper(config => { config.AddMaps(typeof(DependencyInjection).Assembly); });
    services.AddMediatR(config => config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
    services.AddValidators();
    services.AddBehaviors();
    services.AddHttpClient();
    services.AddServices();
    services.AddHostedServices();
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
    services.AddScoped<IBcryptService, BcryptService>();
    return services;
  }

  private static IServiceCollection AddClients(this IServiceCollection services)
  {
    services.AddScoped<ISmtpEmailSender, SmtpEmailSender>();
    services.AddScoped<IExchangeRateClient, ExchangeRateClient>();
    services.AddSingleton<IRabbitMqClient, RabbitMqClient>();

    return services;
  }

  private static IServiceCollection AddHttpClient(this IServiceCollection services)
  {
    services.AddHttpClient<IExchangeRateClient, ExchangeRateClient>((sp, client) =>
    {
      var exchangeRateSettings = sp.GetRequiredService<IOptions<ExchangeRateSettings>>().Value;
      client.BaseAddress = new Uri(exchangeRateSettings.ApiUrl);
      client.DefaultRequestHeaders.Add("Accept", "application/json");
    });
    services.AddHttpClient<ILLMProcessorClient, LLMProcessorClient>((sp, client) =>
    {
      var llmProcessorSettings = sp.GetRequiredService<IOptions<LLMProcessorSettings>>().Value;
      client.BaseAddress = new Uri(llmProcessorSettings.ApiUrl);
      client.DefaultRequestHeaders.Add("Authorization", $"Bearer {llmProcessorSettings.Token}");
      client.DefaultRequestHeaders.Add("Accept", "application/json");
    });
    return services;
  }

  private static IServiceCollection AddHostedServices(this IServiceCollection services)
  {
    services.AddHostedService<ExchangeRateBackgroundJob>();
    services.AddSingleton<ExchangeRateRunSignal>();
    services.AddHostedService<RabbitMqConsumerServiceBackgroundJob>();
    return services;
  }
}
