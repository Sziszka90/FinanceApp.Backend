namespace FinanceApp.Presentation.WebApi.Extensions;

public static class HostEnvironmentExtensions
{
  #region Methods

  public static bool IsTesting(this IHostEnvironment environment)
  {
    return environment.EnvironmentName == "Testing";
  }

  #endregion
}
