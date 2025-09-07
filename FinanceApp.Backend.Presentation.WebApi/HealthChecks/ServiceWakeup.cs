using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.Extensions.Caching.Distributed;
using Polly;

namespace FinanceApp.Backend.Presentation.WebApi.HealthChecks;

public class ServiceWakeup
{
  private readonly FinanceAppDbContext _dbContext;
  private readonly IDistributedCache _cache;
  private readonly ILLMProcessorClient _llmProcessorClient;

  public ServiceWakeup(
    FinanceAppDbContext dbContext,
    IDistributedCache cache,
    ILLMProcessorClient llmProcessorClient)
  {
    _dbContext = dbContext;
    _cache = cache;
    _llmProcessorClient = llmProcessorClient;
  }

  public async Task<bool> WakeupAsync()
  {
    var retryPolicy = Policy
      .Handle<Exception>()
      .WaitAndRetryAsync(
      [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(15)
      ]);

    bool dbOk;
    try
    {
      dbOk = await retryPolicy.ExecuteAsync(async () =>
      {
        return await _dbContext.Database.CanConnectAsync();
      });
    }
    catch (Exception)
    {
      dbOk = false;
    }

    if (!dbOk)
    {
      return false;
    }

    bool cacheOk;
    try
    {
      cacheOk = await retryPolicy.ExecuteAsync(async () =>
      {
        var testKey = "wakeup_test";
        await _cache.SetStringAsync(testKey, "ok");
        var value = await _cache.GetStringAsync(testKey);
        return value == "ok";
      });
    }
    catch (Exception)
    {
      cacheOk = false;
    }

    if (!cacheOk)
    {
      return false;
    }

    bool llmOk;
    try
    {
      llmOk = await retryPolicy.ExecuteAsync(async () =>
      {
        var response = await _llmProcessorClient.WakeupAsync();
        return response.Data;
      });
    }
    catch (Exception)
    {
      llmOk = false;
    }

    if (!llmOk)
    {
      return false;
    }
    return true;
  }
}
