using System.Net;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Clients;
using FinanceApp.Backend.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace FinanceApp.Backend.Testing.Unit.ClientTests;

public class LLMProcessorClientTests : TestBase
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

    var client = new LLMProcessorClient(
      new Mock<ILogger<ILLMProcessorClient>>().Object,
      new HttpClient(HttpMessageHandlerMock.Object) { BaseAddress = new Uri("https://api.llmprocessor.com/") });

    // act
    var result = await client.WakeupAsync();

    // assert
    Assert.True(result.IsSuccess);
    Assert.EndsWith("/wakeup", requestUri);
  }
}
