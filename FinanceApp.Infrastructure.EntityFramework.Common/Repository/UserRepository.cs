using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class UserRepository : GenericRepository<Domain.Entities.User>, IUserRepository
{
  private readonly IFilteredQueryProvider _filteredQueryProvider;
  private readonly FinanceAppDbContext _dbContext;

  /// <inheritdoc />
  public UserRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider
  ) : base(dbContext, filteredQueryProvider)
  {
    _dbContext = dbContext;
    _filteredQueryProvider = filteredQueryProvider;
  }

  public async Task<Domain.Entities.User?> GetByUserNameAsync(string userName, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    if (noTracking)
    {
      return await _filteredQueryProvider.Query<Domain.Entities.User>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(user => user.UserName == userName, cancellationToken);
    }

    return await _filteredQueryProvider.Query<Domain.Entities.User>()
                          .FirstOrDefaultAsync(user => user.UserName == userName, cancellationToken);
  }
}
