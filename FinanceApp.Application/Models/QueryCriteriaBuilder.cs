using System.Linq.Expressions;
using FinanceApp.Application.Extensions;
using FinanceApp.Domain.Common;

namespace FinanceApp.Application.Models;

/// <summary>
/// Builder for query criteria.
/// </summary>
public class QueryCriteriaBuilder<T> where T : BaseEntity
{
  private readonly List<Expression<Func<T, bool>>> _wheres = [];
  private readonly Dictionary<string, Expression<Func<T, object>>> _orderByMappings = new();
  private readonly List<string> _includes = [];
  private Func<IQueryable<T>, IOrderedQueryable<T>>? _orderBy;

  public QueryCriteriaBuilder<T> AddOrderByMapping(string sortingKey, Expression<Func<T, object>> keySelector)
  {
    _orderByMappings.Add(sortingKey.ToLower(), keySelector);
    return this;
  }

  /// <summary>
  /// Sets the order function for the query.
  /// </summary>
  /// <param name="sort"></param>
  /// <returns>Query Criteria Builder</returns>
  /// <exception cref="InvalidOperationException"></exception>
  public QueryCriteriaBuilder<T> OrderBy(string sort)
  {
    _orderBy = q =>
               {
                 var sortList = sort.Split(',');
                 var count = 0;
                 foreach (var sortItem in sortList)
                 {
                   var sortBy = sortItem.Substring(1);
                   var sortDirection = sortItem.Substring(0, 1);
                   var sortExpression = _orderByMappings[sortBy.ToLower()];
                   q = sortDirection switch
                   {
                     "+" => count == 0 ? q.OrderBy(sortExpression) : ((IOrderedQueryable<T>)q).ThenBy(sortExpression),
                     "-" => count == 0 ? q.OrderByDescending(sortExpression) : ((IOrderedQueryable<T>)q).ThenByDescending(sortExpression),
                     _ => throw new InvalidOperationException("Invalid sort direction")
                   };
                   count++;
                 }

                 return (IOrderedQueryable<T>)q;
               };
    return this;
  }

  /// <summary>
  /// Sets the where criteria
  /// </summary>
  /// <param name="expression">Where expression</param>
  /// <param name="condition">The expression is added if it's true</param>
  /// <returns>Query Criteria Builder</returns>
  public QueryCriteriaBuilder<T> Where(Expression<Func<T, bool>> expression, bool? condition = null)
  {
    if (condition is null or true)
    {
      _wheres.Add(expression);
    }

    return this;
  }

  /// <summary>
  /// Include naviation property. Supports chained properties.
  /// </summary>
  /// <param name="include">Include expression</param>
  /// <returns>Query Criteria Builder</returns>
  public QueryCriteriaBuilder<T> Include(Expression<Func<T, object>> include)
  {
    _includes.Add(include.GetPropertyPath());
    return this;
  }

  /// <summary>
  /// Include navigation property to the query by property as a string.
  /// </summary>
  /// <param name="include">Include expression</param>
  /// <returns>Query Criteria Builder</returns>
  public QueryCriteriaBuilder<T> Include(string include)
  {
    _includes.Add(include);
    return this;
  }

  /// <summary>
  /// Create a Query Criteria based the configured builder
  /// </summary>
  /// <returns>The configured Query Criteria</returns>
  public QueryCriteria<T> Build()
  {
    return new QueryCriteria<T>
    {
      Wheres = _wheres,
      IncludesWithPropertyPath = _includes,
      OrderBy = _orderBy
    };
  }
}
