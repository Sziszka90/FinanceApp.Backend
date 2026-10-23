using System.Net;
using System.Text;
using System.Text.Json;
using FinanceApp.Backend.Application.Clients.HttpClients;
using FinanceApp.Backend.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace FinanceApp.Backend.Testing.Unit.ClientTests;

// Test helper classes
public class TestableHttpClientBase : HttpClientBase<TestableHttpClientBase>
{
  public TestableHttpClientBase(ILogger<TestableHttpClientBase> logger, HttpClient httpClient)
    : base(logger, httpClient)
  { }
}

public class HttpClientBaseTests : TestBase, IDisposable
{
  private readonly TestableHttpClientBase _httpClientBase;
  private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
  private readonly HttpClient _httpClient;

  public HttpClientBaseTests()
  {
    _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
    _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
    {
      BaseAddress = new Uri("https://api.test.com/")
    };
    _httpClientBase = new TestableHttpClientBase(CreateLoggerMock<TestableHttpClientBase>().Object, _httpClient);
  }

  public class GetAsyncTests : HttpClientBaseTests
  {
    [Fact]
    public async Task GetAsync_WithValidResponse_ShouldReturnDeserializedObject()
    {
      // arrange
      const string endpoint = "test-endpoint";
      var expectedResponse = new TestResponse { Id = 1, Name = "Test" };
      var jsonResponse = JsonSerializer.Serialize(expectedResponse);

      _httpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
          });

      // act
      var result = await _httpClientBase.GetAsync<TestResponse>(endpoint);

      // assert
      Assert.NotNull(result);
      Assert.Equal(expectedResponse.Id, result.Id);
      Assert.Equal(expectedResponse.Name, result.Name);
    }

    [Fact]
    public async Task GetAsync_WithUnsuccessfulStatusCode_ShouldThrowHttpClientException()
    {
      // arrange
      const string endpoint = "test-endpoint";

      _httpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.BadRequest
          });

      // act & assert
      var exception = await Assert.ThrowsAsync<HttpClientException>(
          () => _httpClientBase.GetAsync<TestResponse>(endpoint));
      Assert.Equal("GET", exception.Operation);
      Assert.Equal(endpoint, exception.Endpoint);
      Assert.Contains("External service call failed", exception.Message);
    }

    [Fact]
    public async Task GetAsync_WithInvalidJson_ShouldThrowHttpClientException()
    {
      // arrange
      const string endpoint = "test-endpoint";
      const string invalidJson = "invalid json";

      _httpMessageHandlerMock.Protected()
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
          () => _httpClientBase.GetAsync<TestResponse>(endpoint));

      // assert
      Assert.Equal("GET", exception.Operation);
      Assert.Equal(endpoint, exception.Endpoint);
      Assert.Contains("An error occurred while making the request", exception.Message);
    }

    [Fact]
    public async Task GetAsync_WithNullResponse_ShouldThrowHttpClientException()
    {
      // arrange
      const string endpoint = "test-endpoint";
      const string nullJson = "null";

      _httpMessageHandlerMock.Protected()
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
          () => _httpClientBase.GetAsync<TestResponse>(endpoint));

      // assert
      Assert.Equal("GET_DESERIALIZE", exception.Operation);
      Assert.Equal(endpoint, exception.Endpoint);
      Assert.Contains("Failed to deserialize response", exception.Message);
    }

    [Fact]
    public async Task GetAsync_WithHttpClientException_ShouldThrowHttpClientException()
    {
      // arrange
      const string endpoint = "test-endpoint";

      _httpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ThrowsAsync(new HttpRequestException("Network error"));

      // act
      var exception = await Assert.ThrowsAsync<HttpClientException>(
          () => _httpClientBase.GetAsync<TestResponse>(endpoint));

      // assert
      Assert.Equal("GET", exception.Operation);
      Assert.Equal(endpoint, exception.Endpoint);
      Assert.Contains("An error occurred while making the request", exception.Message);
      Assert.IsType<HttpRequestException>(exception.InnerException);
    }
  }

  public class GetAsyncWithDataTests : HttpClientBaseTests
  {
    [Fact]
    public async Task GetAsyncWithData_WithValidRequestAndResponse_ShouldReturnDeserializedObject()
    {
      // arrange
      const string endpoint = "test-endpoint";
      var requestData = new TestRequest { Query = "test query" };
      var expectedResponse = new TestResponse { Id = 1, Name = "Test" };
      var jsonResponse = JsonSerializer.Serialize(expectedResponse);

      _httpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get &&
                  req.RequestUri!.ToString().Contains(endpoint)),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
          });

      // act
      var result = await _httpClientBase.GetAsync<TestRequest, TestResponse>(endpoint, requestData);

      // assert
      Assert.NotNull(result);
      Assert.Equal(expectedResponse.Id, result.Id);
      Assert.Equal(expectedResponse.Name, result.Name);
    }

    [Fact]
    public async Task GetAsyncWithData_WithUnsuccessfulStatusCode_ShouldThrowHttpClientException()
    {
      // arrange
      const string endpoint = "test-endpoint";
      var requestData = new TestRequest { Query = "test query" };

      _httpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.InternalServerError
          });

      // act & assert
      var exception = await Assert.ThrowsAsync<HttpClientException>(
        () => _httpClientBase.GetAsync<TestRequest, TestResponse>(endpoint, requestData));
      Assert.Equal("GET_WITH_DATA", exception.Operation);
      Assert.Equal(endpoint, exception.Endpoint);
    }

    [Fact]
    public async Task GetAsyncWithData_WithInvalidJson_ShouldThrowHttpClientException()
    {
      // arrange
      const string endpoint = "test-endpoint";
      var requestData = new TestRequest { Query = "test query" };
      const string invalidJson = "invalid json";

      _httpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(invalidJson, Encoding.UTF8, "application/json")
          });

      // act & assert
      var exception = await Assert.ThrowsAsync<HttpClientException>(
          () => _httpClientBase.GetAsync<TestRequest, TestResponse>(endpoint, requestData));
      Assert.Equal("GET_WITH_DATA", exception.Operation);
      Assert.Equal(endpoint, exception.Endpoint);
      Assert.Contains("An error occurred while making the request", exception.Message);
    }

    [Fact]
    public async Task GetAsyncWithData_WithNullResponse_ShouldThrowHttpClientException()
    {
      // arrange
      const string endpoint = "test-endpoint";
      var requestData = new TestRequest { Query = "test query" };
      const string nullJson = "null";

      _httpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(nullJson, Encoding.UTF8, "application/json")
          });

      // act & assert
      var exception = await Assert.ThrowsAsync<HttpClientException>(
          () => _httpClientBase.GetAsync<TestRequest, TestResponse>(endpoint, requestData));
      Assert.Equal("GET_WITH_DATA_DESERIALIZE", exception.Operation);
      Assert.Equal(endpoint, exception.Endpoint);
      Assert.Contains("Failed to deserialize response", exception.Message);
    }

    [Fact]
    public async Task GetAsyncWithData_WithException_ShouldThrowHttpClientException()
    {
      // arrange
      const string endpoint = "test-endpoint";
      var requestData = new TestRequest { Query = "test query" };

      _httpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ThrowsAsync(new HttpRequestException("Network error"));

      // act & assert
      var exception = await Assert.ThrowsAsync<HttpClientException>(
          () => _httpClientBase.GetAsync<TestRequest, TestResponse>(endpoint, requestData));
      Assert.Equal("GET_WITH_DATA", exception.Operation);
      Assert.Equal(endpoint, exception.Endpoint);
      Assert.Contains("An error occurred while making the request", exception.Message);
      Assert.IsType<HttpRequestException>(exception.InnerException);
    }
  }

  public class PostAsyncTests : HttpClientBaseTests
  {
    [Fact]
    public async Task PostAsync_WithValidRequestAndResponse_ShouldReturnDeserializedObject()
    {
      // arrange
      const string endpoint = "test-endpoint";
      var requestData = new TestRequest { Query = "test query" };
      var expectedResponse = new TestResponse { Id = 1, Name = "Test" };
      var jsonResponse = JsonSerializer.Serialize(expectedResponse);

      _httpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post &&
                  req.RequestUri!.ToString().Contains(endpoint)),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
          });

      // act
      var result = await _httpClientBase.PostAsync<TestRequest, TestResponse>(endpoint, requestData);

      // assert
      Assert.NotNull(result);
      Assert.Equal(expectedResponse.Id, result.Id);
      Assert.Equal(expectedResponse.Name, result.Name);
    }

    [Fact]
    public async Task PostAsync_WithUnsuccessfulStatusCode_ShouldThrowHttpClientException()
    {
      // arrange
      const string endpoint = "test-endpoint";
      var requestData = new TestRequest { Query = "test query" };

      _httpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.Unauthorized
          });

      // act & assert
      var exception = await Assert.ThrowsAsync<HttpClientException>(
          () => _httpClientBase.PostAsync<TestRequest, TestResponse>(endpoint, requestData));
      Assert.Equal("POST", exception.Operation);
      Assert.Equal(endpoint, exception.Endpoint);
    }

    [Fact]
    public async Task PostAsync_WithNullResponse_ShouldThrowHttpClientException()
    {
      // arrange
      const string endpoint = "test-endpoint";
      var requestData = new TestRequest { Query = "test query" };
      const string nullJson = "null";

      _httpMessageHandlerMock.Protected()
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
          () => _httpClientBase.PostAsync<TestRequest, TestResponse>(endpoint, requestData));

      // assert
      Assert.Equal("POST_DESERIALIZE", exception.Operation);
      Assert.Equal(endpoint, exception.Endpoint);
      Assert.Contains("Failed to deserialize response", exception.Message);
    }

    [Fact]
    public async Task PostAsync_WithInvalidJson_ShouldThrowHttpClientException()
    {
      // arrange
      const string endpoint = "test-endpoint";
      var requestData = new TestRequest { Query = "test query" };
      const string invalidJson = "invalid json";

      _httpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(new HttpResponseMessage
          {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(invalidJson, Encoding.UTF8, "application/json")
          });

      // act & assert
      var exception = await Assert.ThrowsAsync<HttpClientException>(
          () => _httpClientBase.PostAsync<TestRequest, TestResponse>(endpoint, requestData));
      Assert.Equal("POST", exception.Operation);
      Assert.Equal(endpoint, exception.Endpoint);
      Assert.Contains("An error occurred while making the request", exception.Message);
    }

    [Fact]
    public async Task PostAsync_WithException_ShouldThrowHttpClientException()
    {
      // arrange
      const string endpoint = "test-endpoint";
      var requestData = new TestRequest { Query = "test query" };

      _httpMessageHandlerMock.Protected()
          .Setup<Task<HttpResponseMessage>>("SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ThrowsAsync(new TaskCanceledException("Request timeout"));

      // act & assert
      var exception = await Assert.ThrowsAsync<HttpClientException>(
          () => _httpClientBase.PostAsync<TestRequest, TestResponse>(endpoint, requestData));
      Assert.Equal("POST", exception.Operation);
      Assert.Equal(endpoint, exception.Endpoint);
      Assert.Contains("An error occurred while making the request", exception.Message);
      Assert.IsType<TaskCanceledException>(exception.InnerException);
    }
  }

  private class TestRequest
  {
    public string Query { get; set; } = string.Empty;
  }

  private class TestResponse
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
  }

  public void Dispose()
  {
    _httpClient?.Dispose();
  }
}
