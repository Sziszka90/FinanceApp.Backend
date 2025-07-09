using FinanceApp.Application.Abstraction.Repositories;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Infrastructure.EntityFramework.Context;
using EFCore.BulkExtensions;

namespace FinanceApp.EntityFramework.Common.Repository;

public class ExchangeRateRepository : IExchangeRateRepository
{
  private readonly FinanceAppDbContext _dbContext;

  /// <inheritdoc />
  public ExchangeRateRepository(
    FinanceAppDbContext dbContext
  )
  {
    _dbContext = dbContext;
  }

  public async Task<List<Domain.Entities.ExchangeRate>> GetExchangeRatesAsync(CancellationToken cancellationToken = default)
  {
    var exchangeRates = await _dbContext.Set<Domain.Entities.ExchangeRate>().ToListAsync(cancellationToken);
    return exchangeRates;
  }

  public async Task<List<Domain.Entities.ExchangeRate>> CreateBatchedExchangeRatesAsync(List<Domain.Entities.ExchangeRate> rates, CancellationToken cancellationToken = default)
  {
    await _dbContext.BulkInsertAsync(rates, cancellationToken: cancellationToken);

    return rates;
  }
}

