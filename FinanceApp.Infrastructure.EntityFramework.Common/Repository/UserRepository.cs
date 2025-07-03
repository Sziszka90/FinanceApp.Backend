using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class UserRepository : GenericRepository<Domain.Entities.User>, IUserRepository
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

  public async Task<Domain.Entities.User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
  {
    return await _filteredQueryProvider.Query<Domain.Entities.User>()
                          .FirstOrDefaultAsync(user => user.UserName == userName, cancellationToken);
  }

  public async Task<Domain.Entities.User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
  {
    return await _filteredQueryProvider.Query<Domain.Entities.User>()
                          .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
  }
}
