namespace FinanceApp.Application.Abstraction.Repositories;

public interface IUserRepository : IRepository<Domain.Entities.User>
{
  /// <summary>
  /// Retrieves a user by their username.
  /// </summary>
  /// <param name="userName"></param>
  /// <param name="cancellationToken"></param>
  /// <returns>User</returns>
  public Task<Domain.Entities.User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Retrieves a user by their email address.
  /// </summary>
  /// <param name="email"></param>
  /// <param name="cancellationToken"></param>
  /// <returns>User</returns>
  public Task<Domain.Entities.User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
}
