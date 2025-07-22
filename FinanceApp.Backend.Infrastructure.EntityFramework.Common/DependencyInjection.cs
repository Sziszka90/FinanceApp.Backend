using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using FinanceApp.Backend.Infrastructure.EntityFramework.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common;

public static class DependencyInjection
{
  public static IServiceCollection AddEntityFrameworkCorePersistence(this IServiceCollection services)
  {
    services.AddScoped<IFilteredQueryProvider, FilteredQueryProvider.FilteredQueryProvider>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
    services.AddScoped(typeof(ITransactionGroupRepository), typeof(TransactionGroupRepository));
    services.AddScoped(typeof(ITransactionRepository), typeof(TransactionRepository));
    services.AddScoped(typeof(IRepository<Domain.Entities.User>), typeof(GenericRepository<Domain.Entities.User>));
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();

    return services;
  }

  public static IServiceCollection AddDatabaseContext(this IServiceCollection services)
  {
    services.AddScoped(sp => sp.GetRequiredService<IScopedContextFactory<FinanceAppDbContext>>()
                               .CreateDbContext());
    return services;
  }
}
