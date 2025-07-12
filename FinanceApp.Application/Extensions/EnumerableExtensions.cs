namespace FinanceApp.Application.Extensions;

public static class EnumerableExtensions
{
  /// <summary>
  /// Creates a dictionary from a list. If the list contains multiple keys with the same value, a counter will be
  /// concatenated to the subsequent keys.
  /// </summary>
  /// <typeparam name="TSource"></typeparam>
  /// <typeparam name="TElement"></typeparam>
  /// <param name="list"></param>
  /// <param name="keySelector"></param>
  /// <param name="valueSelector"></param>
  /// <returns>Dictionary</returns>
  public static Dictionary<string, TElement> ToUniqueDictionaryWithCounters<TSource, TElement>(this IEnumerable<TSource> list, Func<TSource, string> keySelector, Func<TSource, TElement> valueSelector)
  {
    var dictionary = new Dictionary<string, TElement>();
    var counters = new Dictionary<string, int>();

    foreach (var item in list)
    {
      var key = keySelector(item);
      if (dictionary.TryGetValue(key, out _))
      {
        if (!counters.TryGetValue(key, out _))
        {
          counters[key] = 0;
        }

        counters[key]++;
        key = $"{key}_{counters[key]}";
      }

      dictionary.Add(key, valueSelector(item));
    }

    return dictionary;
  }
}
