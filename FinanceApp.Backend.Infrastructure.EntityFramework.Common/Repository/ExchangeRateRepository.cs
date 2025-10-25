using EFCore.BulkExtensions;
using FinanceApp.Backend.Application.Abstraction.Repositories;
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
  /// <inheritdoc />
  public ExchangeRateRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider,
    ISqlQueryBuilder sqlQueryBuilder) : base(dbContext, filteredQueryProvider)
  {
    _sqlQueryBuilder = sqlQueryBuilder;
  }

  /// <inheritdoc />
  public async Task<List<ExchangeRate>> GetExchangeRatesAsync(bool noTracking = false, CancellationToken cancellationToken = default)
  {
    var query = _dbContext.Set<ExchangeRate>().AsQueryable();

    if (noTracking)
    {
      query = query.AsNoTracking();
    }

    return await query.ToListAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task<List<ExchangeRate>> GetActualExchangeRatesAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.Set<ExchangeRate>()
      .Where(x => x.Actual)
      .ToListAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task<List<ExchangeRate>> BatchCreateExchangeRatesAsync(List<ExchangeRate> rates, CancellationToken cancellationToken = default)
  {
    await _dbContext.BulkInsertAsync(rates, cancellationToken: cancellationToken);
    return rates;
  }

  /// <inheritdoc />
  public async Task<List<ExchangeRate>> GetExchangeRatesByDateRangeAsync(DateTimeOffset date, CancellationToken cancellationToken = default)
  {
    var providerName = _dbContext.Database.ProviderName ?? throw new InvalidOperationException("Database provider name is null");
    var sql = _sqlQueryBuilder.BuildGetExchangeRatesByDateRangeQuery(providerName);

    return await _dbContext.ExchangeRate.FromSqlRaw(
      sql,
      new SqlParameter("@date", date)).ToListAsync(cancellationToken);
  }
}

