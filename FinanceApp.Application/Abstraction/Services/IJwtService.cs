namespace FinanceApp.Application.Abstraction.Services;

public interface IJwtService
{
  #region Methods

  public string GenerateToken(string username);

  #endregion
}