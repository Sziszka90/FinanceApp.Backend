using System.Net;
using System.Text;
using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Dtos.ExchangeRateDtos;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace FinanceApp.Backend.Testing.Unit.ClientTests;

public class ExchangeRateClientTests : TestBase
{
  private readonly Mock<ILogger<IExchangeRateClient>> _exchangeRateLoggerMock = new Mock<ILogger<IExchangeRateClient>>();
  private ExchangeRateClient _exchangeRateClient;
  private readonly Mock<IOptions<ExchangeRateSettings>> _exchangeRateOptionsMock = new Mock<IOptions<ExchangeRateSettings>>();


  public ExchangeRateClientTests()
  {
    var exchangeRateSettings = new ExchangeRateSettings
    {
      ApiEndpoint = "https://api.exchangerate.com/v1/latest?app_id=",
      AppId = "test-app-id"
    };

    _exchangeRateOptionsMock.Setup(x => x.Value).Returns(exchangeRateSettings);

    var httpClient = new HttpClient(HttpMessageHandlerMock.Object);

    _exchangeRateClient = new ExchangeRateClient(
        _exchangeRateLoggerMock.Object,
        httpClient,
        _exchangeRateOptionsMock.Object);
  }

  public class GetExchangeRatesAsyncTests : ExchangeRateClientTests
  {
    [Fact]
    public async Task GetExchangeRatesAsync_WithValidResponse_ShouldReturnSuccessResult()
    {
      // arrange
      var responseDto = new ExchangeRateResponseDto
      {
        Base = "USD",
        Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.85m },
                    { "GBP", 0.73m },
                    { "JPY", 110.25m },
                    { "USD", 1.0m }
                },
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        Disclaimer = "Test disclaimer",
        License = "Test license"
      };

      var jsonResponse = JsonSerializer.Serialize(responseDto);

      HttpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString().Contains(_exchangeRateOptionsMock.Object.Value.ApiEndpoint) &&
                req.RequestUri!.ToString().Contains(_exchangeRateOptionsMock.Object.Value.AppId)),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });

      // act
      var result = await _exchangeRateClient.GetExchangeRatesAsync();

      // assert
      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Data);
      Assert.NotEmpty(result.Data);

      // Verify that cross-currency rates are calculated
      var usdToEur = result.Data.FirstOrDefault(r => r.BaseCurrency == "USD" && r.TargetCurrency == "EUR");
      Assert.NotNull(usdToEur);
      Assert.Equal(0.85m, usdToEur.Rate);

      var eurToUsd = result.Data.FirstOrDefault(r => r.BaseCurrency == "EUR" && r.TargetCurrency == "USD");
      Assert.NotNull(eurToUsd);
      Assert.Equal(1 / 0.85m, eurToUsd.Rate, precision: 5);

      // Verify cross-currency calculation (EUR to GBP)
      var eurToGbp = result.Data.FirstOrDefault(r => r.BaseCurrency == "EUR" && r.TargetCurrency == "GBP");
      Assert.NotNull(eurToGbp);
      Assert.Equal(0.73m / 0.85m, eurToGbp.Rate, precision: 5);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithEmptyRates_ShouldReturnSuccessResultWithZeroRates()
    {
      // arrange
      var responseDto = new ExchangeRateResponseDto
      {
        Base = "USD",
        Rates = new Dictionary<string, decimal>(),
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        Disclaimer = "Test disclaimer",
        License = "Test license"
      };

      var jsonResponse = JsonSerializer.Serialize(responseDto);

      HttpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });

      // act
      var result = await _exchangeRateClient.GetExchangeRatesAsync();

      // assert
      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Data);

      // All rates should be 0 when no rates are provided
      Assert.All(result.Data, rate => Assert.Equal(0m, rate.Rate));
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithHttpClientException_ShouldThrowException()
    {
      // arrange
      HttpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.InternalServerError
        });

      // act & assert
      var exception = await Assert.ThrowsAsync<HttpClientException>(
        () => _exchangeRateClient.GetExchangeRatesAsync());

      Assert.Equal("GET", exception.Operation);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithNetworkException_ShouldThrowHttpClientException()
    {
      // arrange
      HttpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ThrowsAsync(new HttpRequestException("Network timeout"));

      // act
      var exception = await Assert.ThrowsAsync<HttpClientException>(
          () => _exchangeRateClient.GetExchangeRatesAsync());

      // assert
      Assert.Equal("GET", exception.Operation);
      Assert.Contains("An error occurred while making the request", exception.Message);
      Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_ShouldUseCorrectEndpoint()
    {
      // arrange
      var responseDto = new ExchangeRateResponseDto
      {
        Base = "USD",
        Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
      };

      var jsonResponse = JsonSerializer.Serialize(responseDto);
      string? capturedUri = null;

      HttpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
        {
          capturedUri = request.RequestUri?.ToString();
        })
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });

      // act
      await _exchangeRateClient.GetExchangeRatesAsync();

      // assert
      Assert.NotNull(capturedUri);
      Assert.Contains(_exchangeRateOptionsMock.Object.Value.ApiEndpoint, capturedUri);
      Assert.Contains(_exchangeRateOptionsMock.Object.Value.AppId, capturedUri);
    }
  }

  public class CalculateAllRatesFromUsdBaseTests : ExchangeRateClientTests
  {
    [Fact]
    public async Task CalculateAllRatesFromUsdBase_WithValidUsdRates_ShouldCalculateAllPairs()
    {
      // arrange
      var usdRates = new Dictionary<string, decimal>
      {
          { "EUR", 0.85m },
          { "GBP", 0.73m },
          { "USD", 1.0m }
      };

      var responseDto = new ExchangeRateResponseDto
      {
        Base = "USD",
        Rates = usdRates
      };

      var jsonResponse = JsonSerializer.Serialize(responseDto);

      HttpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });

      // act
      var result = await _exchangeRateClient.GetExchangeRatesAsync();

      // assert
      Assert.True(result.IsSuccess);
      var rates = result.Data!;

      // Test USD as base currency
      var usdToEur = rates.FirstOrDefault(r => r.BaseCurrency == "USD" && r.TargetCurrency == "EUR");
      Assert.NotNull(usdToEur);
      Assert.Equal(0.85m, usdToEur.Rate);

      // Test non-USD as base currency
      var eurToUsd = rates.FirstOrDefault(r => r.BaseCurrency == "EUR" && r.TargetCurrency == "USD");
      Assert.NotNull(eurToUsd);
      Assert.Equal(1 / 0.85m, eurToUsd.Rate, precision: 5);

      // Test cross-currency calculation
      var eurToGbp = rates.FirstOrDefault(r => r.BaseCurrency == "EUR" && r.TargetCurrency == "GBP");
      Assert.NotNull(eurToGbp);
      Assert.Equal(0.73m / 0.85m, eurToGbp.Rate, precision: 5);

      // Verify no same currency pairs
      Assert.DoesNotContain(rates, r => r.BaseCurrency == r.TargetCurrency);
    }

    [Fact]
    public async Task CalculateAllRatesFromUsdBase_WithZeroRate_ShouldReturnZeroRate()
    {
      // arrange
      var usdRates = new Dictionary<string, decimal>
      {
          { "EUR", 0m },
          { "USD", 1.0m }
      };

      var responseDto = new ExchangeRateResponseDto
      {
        Base = "USD",
        Rates = usdRates
      };

      var jsonResponse = JsonSerializer.Serialize(responseDto);

      HttpMessageHandlerMock.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
      });

      // act
      var result = await _exchangeRateClient.GetExchangeRatesAsync();

      // assert
      Assert.True(result.IsSuccess);
      var rates = result.Data!;

      var eurToUsd = rates.FirstOrDefault(r => r.BaseCurrency == "EUR" && r.TargetCurrency == "USD");
      Assert.NotNull(eurToUsd);
      Assert.Equal(0m, eurToUsd.Rate); // Should be 0 when division by zero would occur
    }

    [Fact]
    public async Task CalculateAllRatesFromUsdBase_WithMissingCurrency_ShouldReturnZeroRate()
    {
      // arrange
      var usdRates = new Dictionary<string, decimal>
      {
          { "EUR", 0.85m }
          // GBP is missing
      };

      var responseDto = new ExchangeRateResponseDto
      {
        Base = "USD",
        Rates = usdRates
      };

      var jsonResponse = JsonSerializer.Serialize(responseDto);

      HttpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });

      // act
      var result = await _exchangeRateClient.GetExchangeRatesAsync();

      // assert
      Assert.True(result.IsSuccess);
      var rates = result.Data!;

      // GBP rates should be 0 when not available
      var usdToGbp = rates.FirstOrDefault(r => r.BaseCurrency == "USD" && r.TargetCurrency == "GBP");
      Assert.NotNull(usdToGbp);
      Assert.Equal(0m, usdToGbp.Rate);

      var gbpToEur = rates.FirstOrDefault(r => r.BaseCurrency == "GBP" && r.TargetCurrency == "EUR");
      Assert.NotNull(gbpToEur);
      Assert.Equal(0m, gbpToEur.Rate);
    }
  }
}
