using System.Net;
using System.Net.Http.Json;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Clients;
using FinanceApp.Backend.Application.Dtos.AIAnalytics;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace FinanceApp.Backend.Testing.Unit.ClientTests;

public class AIAnalyticsClientTests : TestBase
{
  [Fact]
  public async Task WakeupAsync_PostsToWakeupEndpoint()
  {
    // arrange
    string? requestUri = null;
    HttpMessageHandlerMock.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
      .Callback<HttpRequestMessage, CancellationToken>((request, _) => requestUri = request.RequestUri?.ToString())
      .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

    var client = new AIAnalyticsClient(
      new Mock<ILogger<IAIAnalyticsClient>>().Object,
      new HttpClient(HttpMessageHandlerMock.Object) { BaseAddress = new Uri("https://api.aianalytics.com/") });

    // act
    var result = await client.WakeupAsync();

    // assert
    Assert.True(result.IsSuccess);
    Assert.EndsWith("/wakeup", requestUri);
  }

  [Fact]
  public async Task GenerateMonthlyInsightsAsync_ForwardsUserTokenAndDeserializesResponse()
  {
    HttpRequestMessage? sentRequest = null;
    HttpMessageHandlerMock.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
      .Callback<HttpRequestMessage, CancellationToken>((request, _) => sentRequest = request)
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = JsonContent.Create(new MonthlyInsightsResponseDto
        {
          Year = 2026,
          Month = 12,
          Currency = "USD"
        })
      });

    var client = new AIAnalyticsClient(
      new Mock<ILogger<IAIAnalyticsClient>>().Object,
      new HttpClient(HttpMessageHandlerMock.Object) { BaseAddress = new Uri("https://api.aianalytics.com/") });

    var response = await client.GenerateMonthlyInsightsAsync(
      new MonthlyInsightsRequestDto { Year = 2026, Month = 12 },
      "user-jwt",
      CancellationToken.None);

    Assert.Equal("user-jwt", sentRequest!.Headers.GetValues("X-Backend-User-Token").Single());
    Assert.Equal("/aianalytics/monthly-insights", sentRequest.RequestUri?.PathAndQuery);
    Assert.Equal(2026, response.Year);
    Assert.Equal(12, response.Month);
    Assert.Equal("USD", response.Currency);
  }

  [Fact]
  public async Task GenerateMonthlyInsightsAsync_WhenServiceFails_ThrowsHttpRequestException()
  {
    HttpMessageHandlerMock.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadGateway });

    var client = new AIAnalyticsClient(
      new Mock<ILogger<IAIAnalyticsClient>>().Object,
      new HttpClient(HttpMessageHandlerMock.Object) { BaseAddress = new Uri("https://api.aianalytics.com/") });

    var exception = await Assert.ThrowsAsync<HttpRequestException>(() => client.GenerateMonthlyInsightsAsync(
      new MonthlyInsightsRequestDto { Year = 2026, Month = 12 },
      "user-jwt",
      CancellationToken.None));

    Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
  }
}
