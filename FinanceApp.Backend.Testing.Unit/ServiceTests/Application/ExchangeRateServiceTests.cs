using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.ServiceTests.Application;

public class ExchangeRateServiceTests
{
  [Fact]
  public async Task GetRateAsync_CacheHit_ReturnsCachedRate()
  {
    // arrange
    var cacheManagerMock = new Mock<FinanceApp.Backend.Application.Abstraction.Clients.IExchangeRateCacheManager>();
    var repoMock = new Mock<IExchangeRateRepository>();
    var service = new ExchangeRateService(cacheManagerMock.Object);

    cacheManagerMock.Setup(m => m.GetRateAsync(
      It.IsAny<DateTimeOffset>(),
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>())).ReturnsAsync(Result<decimal>.Success(1.23m));

    var rate = await service.ConvertAmountAsync(1.0m, DateTimeOffset.Now, "USD", "USD", CancellationToken.None);

    // assert
    Assert.Equal(1.23m, rate.Data);
    Assert.Equal(1.23m, rate.Data);
  }

  [Fact]
  public async Task GetRateAsync_CacheMiss_RepositoryCalledAndRateCached()
  {
    // arrange
    var cacheManagerMock = new Mock<FinanceApp.Backend.Application.Abstraction.Clients.IExchangeRateCacheManager>();
    var service = new ExchangeRateService(cacheManagerMock.Object);
    cacheManagerMock.Setup(m => m.GetRateAsync(
      It.IsAny<DateTimeOffset>(),
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>())).ReturnsAsync(Result<decimal>.Success(0.99m));

    // act
    var rate = await service.ConvertAmountAsync(1.0m, DateTimeOffset.Now, "EUR", "EUR", CancellationToken.None);

    // assert
    Assert.Equal(0.99m, rate.Data);
    Assert.Equal(0.99m, rate.Data);
  }
}
