using System.Linq.Expressions;
using System.Security.Claims;
using FinanceApp.Domain.Common;
using FinanceApp.Domain.Interfaces;
using FinanceApp.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Common.FilteredQueryProvider;

public class FilteredQueryProvider : IFilteredQueryProvider
{
  private readonly FinanceAppDbContext _dbContext;
  private readonly IHttpContextAccessor? _httpContextAccessor;

  public FilteredQueryProvider(FinanceAppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
  {
    _dbContext = dbContext;
    _httpContextAccessor = httpContextAccessor;
  }

  public IQueryable<T> Query<T>() where T : BaseEntity
  {
    var userName = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    var set = _dbContext.Set<T>();

    if (userName == null || !typeof(IUserOwned).IsAssignableFrom(typeof(T)))
      return set;

    var predicate = WhereUserName<T>(userName);
    var lambda = IncludeUser<T>();

    return set.Include(lambda).Where(predicate);
  }

  private Expression<Func<T, bool>> WhereUserName<T>(string userName)
  {
    var parameter = Expression.Parameter(typeof(T), "e");

    var userProperty = Expression.Property(parameter, "User");

    var userNameProperty = Expression.Property(userProperty, "UserName");

    var userNameConstant = Expression.Constant(userName);

    var comparison = Expression.Equal(userNameProperty, userNameConstant);

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
