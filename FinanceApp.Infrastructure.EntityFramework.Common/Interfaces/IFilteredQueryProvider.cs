using FinanceApp.Domain.Common;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Interfaces;

public interface IFilteredQueryProvider {
  IQueryable<T> Query<T>() where T : BaseEntity;
}

