using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.Abstraction.Services;

public interface IUserService
{
  /// <summary>
  /// Retrieves the currently active user.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
  /// <returns>A result containing the active user entity.</returns>
  Task<Result<User>> GetActiveUserAsync(CancellationToken cancellationToken);

  /// <summary>
  /// Retrieves the authorization token of the currently active user.
  /// </summary>
  /// <returns>A result containing the authorization token as a string.</returns>
  Result<string> GetActiveUserToken();

  /// <summary>
  /// Retrieves the refresh token of the currently active user.
  /// </summary>
  /// <returns>A result containing the refresh token as a string.</returns>
  Result<string> GetActiveUserRefreshToken();
}
