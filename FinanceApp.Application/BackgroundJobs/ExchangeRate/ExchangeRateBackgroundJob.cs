using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.BackgroundJobs.ExchangeRate;

public class ExchangeRateBackgroundJob : BackgroundService
{
  private readonly ILogger<ExchangeRateBackgroundJob> _logger;
  private readonly IExchangeRateRepository _exchangeRateRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IServiceProvider _serviceProvider;

  public ExchangeRateBackgroundJob(
    ILogger<ExchangeRateBackgroundJob> logger,
    IExchangeRateRepository exchangeRateRepository,
    IUnitOfWork unitOfWork,
    IServiceProvider serviceProvider)
  {
    _logger = logger;
    _exchangeRateRepository = exchangeRateRepository;
    _unitOfWork = unitOfWork;
    _serviceProvider = serviceProvider;
  }

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      try
      {
        using var scope = _serviceProvider.CreateScope();
        var exchangeRateClient = scope.ServiceProvider.GetRequiredService<IExchangeRateClient>();
        _logger.LogWarning("Exchange rate request is disabled for now.");
        /*
        var rates = await exchangeRateClient.GetExchangeRatesAsync();

        if (rates.IsSuccess)
        {
          await _exchangeRateRepository.CreateBatchedExchangeRatesAsync(rates.Data!, cancellationToken);
          await _unitOfWork.SaveChangesAsync(cancellationToken);
          _logger.LogInformation("Exchange rates updated successfully.");
        }
        */
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while fetching exchange rates.");
      }
      // Wait for 1 week before next run
      await Task.Delay(TimeSpan.FromDays(7), cancellationToken);
    }
  }
}
