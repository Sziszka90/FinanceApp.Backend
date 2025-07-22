using FinanceApp.Backend.Domain.Common;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;

public interface IFilteredQueryProvider {
  /// <summary>
  /// Provides a queryable interface for entities of type T.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns>IQueryable<T></returns>
  IQueryable<T> Query<T>() where T : BaseEntity;
}

