using FinanceApp.Backend.Application.Abstraction.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Backend.Infrastructure.RabbitMq;

public static class DependencyInjection
{
  public static IServiceCollection AddRabbitMq(this IServiceCollection services)
  {
    services.AddSingleton<IRabbitMqConnectionManager, RabbitMqConnectionManager>();
    services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
    return services;
  }
}
