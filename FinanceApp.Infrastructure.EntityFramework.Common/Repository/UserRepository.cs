using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class UserRepository : GenericRepository<Domain.Entities.User>, IUserRepository
{
  #region Constructors

  /// <inheritdoc />
  public UserRepository(FinanceAppDbContext dbContext) : base(dbContext) { }

  #endregion

  #region Methods

  public async Task<Domain.Entities.User?> GetByUserNameAsync(string userName, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    if (noTracking)
    {
      return await DbContext.Set<Domain.Entities.User>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(user => user.UserName == userName, cancellationToken);
    }

    return await DbContext.Set<Domain.Entities.User>()
                          .FirstOrDefaultAsync(user => user.UserName == userName, cancellationToken);
  }

  #endregion
}