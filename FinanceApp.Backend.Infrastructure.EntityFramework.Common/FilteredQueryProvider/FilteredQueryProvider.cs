using System.Linq.Expressions;
using System.Security.Claims;
using FinanceApp.Backend.Domain.Common;
using FinanceApp.Backend.Domain.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.FilteredQueryProvider;

public class FilteredQueryProvider : IFilteredQueryProvider
{
  private readonly FinanceAppDbContext _dbContext;
  private readonly IHttpContextAccessor? _httpContextAccessor;

  public FilteredQueryProvider(
    FinanceAppDbContext dbContext,
    IHttpContextAccessor httpContextAccessor)
  {
    _dbContext = dbContext;
    _httpContextAccessor = httpContextAccessor;
  }

  /// <inheritdoc />
  public IQueryable<T> Query<T>() where T : BaseEntity
  {
    var set = _dbContext.Set<T>();
    if (!typeof(IUserOwned).IsAssignableFrom(typeof(T)))
    {
      return set;
    }

    var httpContext = _httpContextAccessor?.HttpContext;
    if (httpContext is null)
    {
      // Background consumers have no HTTP identity; they must scope queries explicitly by owner ID.
      return set;
    }

    var userEmail = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrWhiteSpace(userEmail))
    {
      // Fail closed for HTTP requests without an authenticated user claim.
      return set.Where(_ => false);
    }

    var predicate = WhereUserEmail<T>(userEmail);
    var lambda = IncludeUser<T>();

    return set.Include(lambda).Where(predicate);
  }

  private Expression<Func<T, bool>> WhereUserEmail<T>(string userEmail)
  {
    var parameter = Expression.Parameter(typeof(T), "e");
    var userProperty = Expression.Property(parameter, "User");
    var userEmailProperty = Expression.Property(userProperty, "Email");
    var userEmailConstant = Expression.Constant(userEmail);
    var comparison = Expression.Equal(userEmailProperty, userEmailConstant);
    var predicate = Expression.Lambda<Func<T, bool>>(comparison, parameter);
    return predicate;
  }

  private Expression<Func<T, Domain.Entities.User>> IncludeUser<T>() where T : BaseEntity
  {
    var parameter = Expression.Parameter(typeof(T), "e");
    var userProp = Expression.Property(parameter, "User");
    var lambda = Expression.Lambda<Func<T, Domain.Entities.User>>(userProp, parameter);
    return lambda;
  }


}
