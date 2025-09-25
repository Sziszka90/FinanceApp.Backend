using EFCore.BulkExtensions;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services.Abstraction;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;

public class ExchangeRateRepository : GenericRepository<ExchangeRate>, IExchangeRateRepository
{
  private readonly ISqlQueryBuilder _sqlQueryBuilder;
  private readonly IDatabaseCommandService _databaseCommandService;
  /// <inheritdoc />
  public ExchangeRateRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider,
    ISqlQueryBuilder sqlQueryBuilder,
    IDatabaseCommandService databaseCommandService) : base(dbContext, filteredQueryProvider)
  {
    _sqlQueryBuilder = sqlQueryBuilder;
    _databaseCommandService = databaseCommandService;
  }

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
  public async Task<List<ExchangeRate>> GetActualExchangeRatesAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _dbContext.Set<ExchangeRate>().AsQueryable();

      query = query.Where(x => x.Actual);

      var exchangeRates = await query.ToListAsync(cancellationToken);
      return exchangeRates;
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_ACTUAL_EXCHANGE_RATES", nameof(ExchangeRate), null, ex);
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

  /// <inheritdoc />
  public async Task<List<ExchangeRate>> GetExchangeRatesByDateRangeAsync(DateTimeOffset date, CancellationToken cancellationToken = default)
  {
    try
    {
      var providerName = _dbContext.Database.ProviderName ?? throw new InvalidOperationException("Database provider name is null");
      var sql = _sqlQueryBuilder.BuildGetExchangeRatesByDateRangeQuery(providerName);

      var result = await _dbContext.ExchangeRate.FromSqlRaw(
        sql,
          new SqlParameter("@date", date)).ToListAsync(cancellationToken);

      return result;
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_EXCHANGE_RATES_BY_DATE_RANGE", nameof(ExchangeRate), null, ex);
    }
  }
}

