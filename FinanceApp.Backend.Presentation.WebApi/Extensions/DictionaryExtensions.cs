namespace FinanceApp.Backend.Presentation.WebApi.Extensions;

public static class DictionaryExtensions
{
  public static Dictionary<string, object> KeysToPascalCase(this Dictionary<string, object> dict)
  {
    var result = new Dictionary<string, object>();
    foreach (var kvp in dict)
    {
      var pascalKey = SnakeToPascalCase(kvp.Key);
      result[pascalKey] = kvp.Value;
    }
    return result;
  }

  private static string SnakeToPascalCase(string snake)
  {
    if (string.IsNullOrEmpty(snake))
    {
      return snake;
    }

    var parts = snake.Split('_');
    return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
  }
}
