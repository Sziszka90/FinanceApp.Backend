namespace FinanceApp.Application.Abstraction.Services;

public interface ICurrentUserService
{
  /// <summary>
  /// UserName of the current user.
  /// </summary>
  string UserName { get; }
}
