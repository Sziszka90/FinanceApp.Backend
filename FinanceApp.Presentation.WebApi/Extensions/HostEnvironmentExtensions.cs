namespace FinanceApp.Presentation.WebApi.Extensions;

public static class HostEnvironmentExtensions
{
  public static bool IsTesting(this IHostEnvironment environment)
  {
    return environment.EnvironmentName == "Testing";
  }
}
