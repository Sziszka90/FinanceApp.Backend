using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class SavingRepository : GenericRepository<Saving>, ISavingRepository
{
  #region Constructors

  /// <inheritdoc />
  public SavingRepository(FinanceAppDbContext dbContext) : base(dbContext) { }

  #endregion
}
