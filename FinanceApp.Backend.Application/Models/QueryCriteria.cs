using System.Linq.Expressions;

namespace FinanceApp.Backend.Application.Models;

public class QueryCriteria<T>
{
  /// <summary>
  /// Where conditions of the query. Multiple wheres can be aggregated
  /// </summary>
  public IEnumerable<Expression<Func<T, bool>>>? Wheres { get; set; }

  /// <summary>
  /// A function to make normal query to ordered query
  /// </summary>
  public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; set; }

  /// <summary>
  /// Include expressions to add foreign table data
  /// </summary>
  public IEnumerable<Expression<Func<T, object>>>? Includes { get; set; }

  /// <summary>
  /// Include expressions to add foreign table data with string property paths
  /// </summary>
  public IEnumerable<string>? IncludesWithPropertyPath { get; set; }
}
