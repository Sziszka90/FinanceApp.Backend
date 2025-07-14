using FinanceApp.Application.Abstraction.Repositories;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Infrastructure.EntityFramework.Context;
using EFCore.BulkExtensions;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Common.Repository;
using FinanceApp.Infrastructure.EntityFramework.Common.Interfaces;

namespace FinanceApp.EntityFramework.Common.Repository;

public class ExchangeRateRepository : GenericRepository<ExchangeRate>, IExchangeRateRepository
{
  /// <inheritdoc />
  public ExchangeRateRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider) : base(dbContext, filteredQueryProvider)
  {}

  /// <inheritdoc />
  public async Task<List<ExchangeRate>> GetExchangeRatesAsync(bool noTracking = false, CancellationToken cancellationToken = default)
  {
    var query = _dbContext.Set<ExchangeRate>().AsQueryable();

    if (noTracking)
    {
      query = query.AsNoTracking();
    }

    var exchangeRates = await query.ToListAsync(cancellationToken);
    return exchangeRates;
  }

  /// <inheritdoc />
  public async Task<List<ExchangeRate>> BatchCreateExchangeRatesAsync(List<ExchangeRate> rates, CancellationToken cancellationToken = default)
  {
    await _dbContext.BulkInsertAsync(rates, cancellationToken: cancellationToken);
    return rates;
  }
}

