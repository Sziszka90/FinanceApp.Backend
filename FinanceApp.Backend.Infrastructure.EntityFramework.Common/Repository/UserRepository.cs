using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;

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
    try
    {
      var query = _filteredQueryProvider.Query<User>()
                            .Where(user => user.UserName == userName);

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      return await query.FirstOrDefaultAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_BY_USERNAME", nameof(User), null, ex);
    }
  }

  public async Task<User?> GetUserByEmailAsync(string email, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _filteredQueryProvider.Query<User>()
                            .Where(user => user.Email == email);

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      return await query.FirstOrDefaultAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_BY_EMAIL", nameof(User), null, ex);
    }
  }
}
