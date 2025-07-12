using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Abstraction.Repositories;

public interface IUserRepository : IRepository<User>
{
  /// <summary>
  /// Retrieves a user by their username.
  /// </summary>
  /// <param name="userName"></param>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken"></param>
  /// <returns>User</returns>
  public Task<User?> GetByUserNameAsync(string userName, bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Retrieves a user by their email address.
  /// </summary>
  /// <param name="email"></param>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken"></param>
  /// <returns>User</returns>
  public Task<User?> GetUserByEmailAsync(string email, bool noTracking = false, CancellationToken cancellationToken = default);
}
