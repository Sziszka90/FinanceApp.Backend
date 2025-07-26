using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using EFCore.BulkExtensions;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;

public class ExchangeRateRepository : GenericRepository<ExchangeRate>, IExchangeRateRepository
{
  /// <inheritdoc />
  public ExchangeRateRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider) : base(dbContext, filteredQueryProvider)
  { }

  /// <inheritdoc />
  public async Task<List<ExchangeRate>> GetExchangeRatesAsync(bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _dbContext.Set<ExchangeRate>().AsQueryable();

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      var exchangeRates = await query.ToListAsync(cancellationToken);
      return exchangeRates;
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_ALL", nameof(ExchangeRate), null, ex);
    }
  }

  /// <inheritdoc />
  public async Task<List<ExchangeRate>> BatchCreateExchangeRatesAsync(List<ExchangeRate> rates, CancellationToken cancellationToken = default)
  {
    try
    {
      await _dbContext.BulkInsertAsync(rates, cancellationToken: cancellationToken);
      return rates;
    }
    catch (Exception ex)
    {
      throw new DatabaseException("BULK_INSERT", nameof(ExchangeRate), null, ex);
    }
  }
}

