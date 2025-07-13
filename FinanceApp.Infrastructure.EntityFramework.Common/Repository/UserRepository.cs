using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class UserRepository : GenericRepository<User>, IUserRepository
{
  private readonly IFilteredQueryProvider _filteredQueryProvider;

  /// <inheritdoc />
  public UserRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider
  ) : base(dbContext, filteredQueryProvider)
  {
    _filteredQueryProvider = filteredQueryProvider;
  }

  public async Task<User?> GetByUserNameAsync(string userName, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    var query = _filteredQueryProvider.Query<User>()
                          .Where(user => user.UserName == userName);

    if (noTracking)
    {
      query = query.AsNoTracking();
    }

    return await query.FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<User?> GetUserByEmailAsync(string email, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    var query = _filteredQueryProvider.Query<User>()
                          .Where(user => user.Email == email);

    if (noTracking)
    {
      query = query.AsNoTracking();
    }

    return await query.FirstOrDefaultAsync(cancellationToken);
  }
}
