using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Common.Repository;
using FinanceApp.Infrastructure.EntityFramework.Context;
using FinanceApp.Infrastructure.EntityFramework.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Infrastructure.EntityFramework.Common;

public static class DependencyInjection
{
  #region Methods

  public static IServiceCollection AddEntityFrameworkCorePersistence(this IServiceCollection services)
  {
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
    services.AddScoped(typeof(IRepository<ExpenseTransactionGroup>), typeof(GenericRepository<ExpenseTransactionGroup>));
    services.AddScoped(typeof(IRepository<ExpenseTransaction>), typeof(ExpenseTransactionRepository));
    services.AddScoped(typeof(IRepository<IncomeTransactionGroup>), typeof(GenericRepository<IncomeTransactionGroup>));
    services.AddScoped(typeof(IRepository<IncomeTransaction>), typeof(IncomeTransactionRepository));
    services.AddScoped(typeof(IRepository<Investment>), typeof(GenericRepository<Investment>));
    services.AddScoped(typeof(IRepository<Saving>), typeof(GenericRepository<Saving>));
    services.AddScoped(typeof(IRepository<Domain.Entities.User>), typeof(GenericRepository<Domain.Entities.User>));
    services.AddScoped<IUserRepository, UserRepository>();

    return services;
  }

  public static IServiceCollection AddDatabaseContext(this IServiceCollection services)
  {
    services.AddScoped(sp => sp.GetRequiredService<IScopedContextFactory<FinanceAppDbContext>>()
                               .CreateDbContext());
    return services;
  }

  #endregion
}