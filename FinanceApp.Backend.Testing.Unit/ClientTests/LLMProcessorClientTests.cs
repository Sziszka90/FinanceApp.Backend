using System.Net;
using System.Text;
using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Clients;
using FinanceApp.Backend.Application.Dtos.LLMProcessorDtos;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace FinanceApp.Backend.Testing.Unit.ClientTests;

public class LLMProcessorClientTests : TestBase
{
  private readonly LLMProcessorClient _llmProcessorClient;
  private readonly Mock<ILogger<ILLMProcessorClient>> _llmProcessorLoggerMock;
  private readonly HttpClient _httpClient;
  private readonly LLMProcessorSettings _llmProcessorSettings;
  private readonly Mock<IOptions<LLMProcessorSettings>> _llmProcessorOptionsMock;

  public LLMProcessorClientTests()
  {
    _llmProcessorOptionsMock = new Mock<IOptions<LLMProcessorSettings>>();
    _llmProcessorLoggerMock = new Mock<ILogger<ILLMProcessorClient>>();

    _llmProcessorSettings = new LLMProcessorSettings
    {
      ApiUrl = "https://api.llmprocessor.com/",
      Token = "test-token",
    };

    _llmProcessorOptionsMock.Setup(x => x.Value).Returns(_llmProcessorSettings);

    _httpClient = new HttpClient(HttpMessageHandlerMock.Object)
    {
      BaseAddress = new Uri("https://api.llmprocessor.com/")
    };

    _llmProcessorClient = new LLMProcessorClient(
      _llmProcessorLoggerMock.Object,
      _httpClient,
      _llmProcessorOptionsMock.Object);
  }

  public class MatchTransactionGroupTests : LLMProcessorClientTests
  {
    [Fact]
    public async Task MatchTransactionGroup_WithValidRequest_ShouldReturnSuccessResult()
    {
      // arrange
      var transactionNames = new List<string> { "Amazon Purchase", "Grocery Store", "Gas Station" };
      var existingGroups = new List<string> { "Shopping", "Food", "Transportation" };
      const string userId = "user-123";
      const string correlationId = "corr-456";

      var responseDto = new LLMProcessorResponseDto
      {
        Status = "success",
        CorrelationId = correlationId,
        Message = "Transaction matching completed successfully"
      };

      var jsonResponse = JsonSerializer.Serialize(responseDto);

      HttpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().Contains("/llmProcessor/match-transactions")),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });

      // act
      var result = await _llmProcessorClient.MatchTransactionGroup(
        userId, transactionNames, existingGroups, correlationId);

      // assert
      Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MatchTransactionGroup_ShouldSendCorrectRequestData()
    {
      // arrange
      var transactionNames = new List<string> { "Amazon Purchase", "Grocery Store" };
      var existingGroups = new List<string> { "Shopping", "Food" };
      const string userId = "user-123";
      const string correlationId = "corr-456";

      var responseDto = new LLMProcessorResponseDto
      {
        Status = "success",
        CorrelationId = correlationId,
        Message = "Success"
      };

      var jsonResponse = JsonSerializer.Serialize(responseDto);
      string? capturedRequestContent = null;

      HttpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .Callback<HttpRequestMessage, CancellationToken>(async (request, _) =>
        {
          if (request.Content != null)
          {
            capturedRequestContent = await request.Content.ReadAsStringAsync();
          }
        })
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });

      // act
      await _llmProcessorClient.MatchTransactionGroup(
        userId, transactionNames, existingGroups, correlationId);

      // assert
      Assert.NotNull(capturedRequestContent);

      var sentRequest = JsonSerializer.Deserialize<MatchTransactionRequestDto>(capturedRequestContent,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

      Assert.NotNull(sentRequest);
      Assert.Equal(userId, sentRequest.UserId);
      Assert.Equal(correlationId, sentRequest.CorrelationId);
      Assert.Equal(transactionNames, sentRequest.TransactionNames);
      Assert.Equal(existingGroups, sentRequest.TransactionGroupNames);
    }

    [Fact]
    public async Task MatchTransactionGroup_WithEmptyTransactionNames_ShouldStillWork()
    {
      // arrange
      var transactionNames = new List<string>();
      var existingGroups = new List<string> { "Shopping", "Food" };
      const string userId = "user-123";
      const string correlationId = "corr-456";

      var responseDto = new LLMProcessorResponseDto
      {
        Status = "success",
        CorrelationId = correlationId,
        Message = "Success"
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
      var result = await _llmProcessorClient.MatchTransactionGroup(
        userId, transactionNames, existingGroups, correlationId);

      // assert
      Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MatchTransactionGroup_WithEmptyExistingGroups_ShouldStillWork()
    {
      // arrange
      var transactionNames = new List<string> { "Amazon Purchase", "Grocery Store" };
      var existingGroups = new List<string>();
      const string userId = "user-123";
      const string correlationId = "corr-456";

      var responseDto = new LLMProcessorResponseDto
      {
        Status = "success",
        CorrelationId = correlationId,
        Message = "Success"
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
      var result = await _llmProcessorClient.MatchTransactionGroup(
        userId, transactionNames, existingGroups, correlationId);

      // assert
      Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MatchTransactionGroup_WithHttpError_ShouldThrowHttpClientException()
    {
      // arrange
      var transactionNames = new List<string> { "Amazon Purchase" };
      var existingGroups = new List<string> { "Shopping" };
      const string userId = "user-123";
      const string correlationId = "corr-456";

      HttpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.BadRequest,
          Content = new StringContent("Invalid request", Encoding.UTF8, "application/json")
        });

      // act & assert
      var exception = await Assert.ThrowsAsync<HttpClientException>(
          () => _llmProcessorClient.MatchTransactionGroup(
              userId, transactionNames, existingGroups, correlationId));
      Assert.Equal("POST", exception.Operation);
      Assert.Contains("External service call failed", exception.Message);
    }

    [Fact]
    public async Task MatchTransactionGroup_WithNetworkException_ShouldThrowHttpClientException()
    {
      // arrange
      var transactionNames = new List<string> { "Amazon Purchase" };
      var existingGroups = new List<string> { "Shopping" };
      const string userId = "user-123";
      const string correlationId = "corr-456";

      HttpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ThrowsAsync(new HttpRequestException("Network timeout"));

      // act
      var exception = await Assert.ThrowsAsync<HttpClientException>(
        () => _llmProcessorClient.MatchTransactionGroup(
            userId, transactionNames, existingGroups, correlationId));

      Assert.Equal("POST", exception.Operation);
      Assert.Contains("An error occurred while making the request", exception.Message);
      Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    [Fact]
    public async Task MatchTransactionGroup_WithInvalidJsonResponse_ShouldThrowHttpClientException()
    {
      // arrange
      var transactionNames = new List<string> { "Amazon Purchase" };
      var existingGroups = new List<string> { "Shopping" };
      const string userId = "user-123";
      const string correlationId = "corr-456";

      const string invalidJson = "{ invalid json }";

      HttpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(invalidJson, Encoding.UTF8, "application/json")
          });

      // act
      var exception = await Assert.ThrowsAsync<HttpClientException>(
          () => _llmProcessorClient.MatchTransactionGroup(
              userId, transactionNames, existingGroups, correlationId));

      // assert
      Assert.Equal("POST", exception.Operation);
      Assert.Contains("An error occurred while making the request", exception.Message);
    }

    [Fact]
    public async Task MatchTransactionGroup_WithNullResponse_ShouldThrowHttpClientException()
    {
      // arrange
      var transactionNames = new List<string> { "Amazon Purchase" };
      var existingGroups = new List<string> { "Shopping" };
      const string userId = "user-123";
      const string correlationId = "corr-456";

      const string nullJson = "null";

      HttpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(nullJson, Encoding.UTF8, "application/json")
        });

      // act
      var exception = await Assert.ThrowsAsync<HttpClientException>(
        () => _llmProcessorClient.MatchTransactionGroup(
          userId, transactionNames, existingGroups, correlationId));

      // assert
      Assert.Equal("POST_DESERIALIZE", exception.Operation);
      Assert.Contains("Failed to deserialize response", exception.Message);
    }

    [Fact]
    public async Task MatchTransactionGroup_ShouldUseCorrectEndpoint()
    {
      // arrange
      var transactionNames = new List<string> { "Amazon Purchase" };
      var existingGroups = new List<string> { "Shopping" };
      const string userId = "user-123";
      const string correlationId = "corr-456";

      var responseDto = new LLMProcessorResponseDto
      {
        Status = "success",
        CorrelationId = correlationId,
        Message = "Success"
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
      await _llmProcessorClient.MatchTransactionGroup(
        userId, transactionNames, existingGroups, correlationId);

      // assert
      Assert.NotNull(capturedUri);
      Assert.Contains("/llmProcessor/match-transactions", capturedUri);
    }

    [Fact]
    public async Task MatchTransactionGroup_ShouldSetCorrectContentType()
    {
      // arrange
      var transactionNames = new List<string> { "Amazon Purchase" };
      var existingGroups = new List<string> { "Shopping" };
      const string userId = "user-123";
      const string correlationId = "corr-456";

      var responseDto = new LLMProcessorResponseDto
      {
        Status = "success",
        CorrelationId = correlationId,
        Message = "Success"
      };

      var jsonResponse = JsonSerializer.Serialize(responseDto);
      string? capturedContentType = null;

      HttpMessageHandlerMock.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
        .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
        {
          capturedContentType = request.Content?.Headers.ContentType?.MediaType;
        })
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });

      // act
      await _llmProcessorClient.MatchTransactionGroup(
        userId, transactionNames, existingGroups, correlationId);

      // assert
      Assert.Equal("application/json", capturedContentType);
    }
  }
}
