using System.Net;
using System.Text;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace FinanceApp.Backend.Testing.Unit.ClientTests;

public class ExchangeRateClientTests : TestBase
{
  private readonly Mock<ILogger<IExchangeRateClient>> _logger = new();
  private readonly ExchangeRateClient _client;
  private const string LATEST_ENDPOINT = "https://api.frankfurter.dev/v2/rates?base=USD";
  private const string HISTORICAL_ENDPOINT = "https://api.frankfurter.dev/v2/rates?base=USD&date={date}";

  public ExchangeRateClientTests()
  {
    var settings = Options.Create(new ExchangeRateSettings
    {
      ApiEndpoint = LATEST_ENDPOINT,
      HistoricalEndpoint = HISTORICAL_ENDPOINT
    });
    _client = new ExchangeRateClient(_logger.Object, new HttpClient(HttpMessageHandlerMock.Object), settings);
  }

  [Fact]
  public async Task GetExchangeRatesAsync_WithFrankfurterResponse_CalculatesCrossRatesAndUsesProviderDate()
  {
    HttpMessageHandlerMock.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(JsonResponse("""
        [
          { "date": "2026-10-23", "base": "USD", "quote": "EUR", "rate": 0.85 },
          { "date": "2026-10-23", "base": "USD", "quote": "GBP", "rate": 0.73 },
          { "date": "2026-10-23", "base": "USD", "quote": "HUF", "rate": 350 }
        ]
        """));

    var result = await _client.GetExchangeRatesAsync();

    Assert.True(result.IsSuccess);
    var rates = result.Data!;
    Assert.Equal(0.85m, rates.Single(rate => rate.BaseCurrency == "USD" && rate.TargetCurrency == "EUR").Rate);
    Assert.Equal(1m / 0.85m, rates.Single(rate => rate.BaseCurrency == "EUR" && rate.TargetCurrency == "USD").Rate, 5);
    Assert.Equal(0.73m / 0.85m, rates.Single(rate => rate.BaseCurrency == "EUR" && rate.TargetCurrency == "GBP").Rate, 5);
    Assert.All(rates, rate => Assert.Equal(new DateTimeOffset(2026, 10, 23, 0, 0, 0, TimeSpan.Zero), rate.ValidFrom));
    Assert.All(rates, rate => Assert.True(rate.Actual));
  }

  [Fact]
  public async Task GetExchangeRatesAsync_ForHistoricalDate_UsesFrankfurterDateQueryAndMarksRatesHistorical()
  {
    string? requestedUri = null;
    HttpMessageHandlerMock.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
      .Callback<HttpRequestMessage, CancellationToken>((request, _) => requestedUri = request.RequestUri?.ToString())
      .ReturnsAsync(JsonResponse("""
        [
          { "date": "2026-10-22", "base": "USD", "quote": "EUR", "rate": 0.84 },
          { "date": "2026-10-22", "base": "USD", "quote": "GBP", "rate": 0.72 },
          { "date": "2026-10-22", "base": "USD", "quote": "HUF", "rate": 349 }
        ]
        """));

    var result = await _client.GetExchangeRatesAsync(new DateOnly(2026, 10, 23));

    Assert.True(result.IsSuccess);
    Assert.Contains("/v2/rates?base=USD&date=2026-10-23", requestedUri);
    Assert.All(result.Data!, rate => Assert.False(rate.Actual));
    Assert.All(result.Data!, rate => Assert.Equal(new DateTimeOffset(2026, 10, 22, 0, 0, 0, TimeSpan.Zero), rate.ValidFrom));
  }

  [Fact]
  public async Task GetExchangeRatesAsync_WhenFrankfurterOmitsSupportedCurrency_ThrowsInvalidDataException()
  {
    HttpMessageHandlerMock.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(JsonResponse("""
        [
          { "date": "2026-10-23", "base": "USD", "quote": "EUR", "rate": 0.85 },
          { "date": "2026-10-23", "base": "USD", "quote": "GBP", "rate": 0.73 }
        ]
        """));

    await Assert.ThrowsAsync<InvalidDataException>(() => _client.GetExchangeRatesAsync());
  }

  [Fact]
  public async Task GetExchangeRatesAsync_WhenRequestFails_ThrowsHttpClientException()
  {
    HttpMessageHandlerMock.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

    var exception = await Assert.ThrowsAsync<HttpClientException>(() => _client.GetExchangeRatesAsync());

    Assert.Equal("GET", exception.Operation);
  }

  private static HttpResponseMessage JsonResponse(string json) => new()
  {
    StatusCode = HttpStatusCode.OK,
    Content = new StringContent(json, Encoding.UTF8, "application/json")
  };
}
