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
}
