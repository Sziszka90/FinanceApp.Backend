using System.Text.Json;
using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Clients.HttpClients;

public abstract class HttpClientBase<T> : IHttpClientBase
{
  private readonly ILogger<T> _logger;
  protected readonly HttpClient _httpClient;
  private readonly JsonSerializerOptions _options = new JsonSerializerOptions
  {
    PropertyNameCaseInsensitive = true
  };

  protected HttpClientBase(ILogger<T> logger, HttpClient httpClient)
  {
    _logger = logger;
    _httpClient = httpClient;
  }

  public void SetAuthorizationHeader(string token)
  {
    if (!string.IsNullOrEmpty(token))
    {
      _httpClient.DefaultRequestHeaders.Remove("Authorization");
      _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }
  }

  public async Task<Result<TResponse>> GetAsync<TResponse>(string endpoint)
  {
    try
    {
      var response = await _httpClient.GetAsync(endpoint);

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("Failed to fetch data from external service. Status code: {StatusCode}", response.StatusCode);
        return Result.Failure<TResponse>(ApplicationError.ExternalCallError("External service call failed."));
      }

      var content = await response.Content.ReadAsStringAsync();
      var result = JsonSerializer.Deserialize<TResponse>(content, _options);

      if (result is null)
      {
        _logger.LogError("Failed to deserialize response: {Content}", content);
        return Result.Failure<TResponse>(ApplicationError.ExternalCallError("Failed to deserialize response."));
      }

      _logger.LogDebug("Data fetched successfully: {Content}", content);
      return Result.Success(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred while making a GET request to {Endpoint}", endpoint);
      return Result.Failure<TResponse>(ApplicationError.ExternalCallError("An error occurred while making the request."));
    }
  }

  public async Task<Result<TResponse>> GetAsync<TRequest, TResponse>(string endpoint, TRequest data)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, endpoint)
    {
      Content = new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json")
    };

    try
    {
      var response = await _httpClient.SendAsync(request);

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("Failed to fetch data from external service. Status code: {StatusCode}", response.StatusCode);
        return Result.Failure<TResponse>(ApplicationError.ExternalCallError("External service call failed."));
      }

      var content = await response.Content.ReadAsStringAsync();
      var result = JsonSerializer.Deserialize<TResponse>(content, _options);

      if (result is null)
      {
        _logger.LogError("Failed to deserialize response: {Content}", content);
        return Result.Failure<TResponse>(ApplicationError.ExternalCallError("Failed to deserialize response."));
      }

      _logger.LogDebug("Data fetched successfully: {Content}", content);
      return Result.Success(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred while making a GET request to {Endpoint} with data: {Data}", endpoint, data);
      return Result.Failure<TResponse>(ApplicationError.ExternalCallError("An error occurred while making the request."));
    }
  }

  public async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
  {
    var json = JsonSerializer.Serialize(data);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

    try
    {
      var response = await _httpClient.PostAsync(endpoint, content);

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("Failed to post data to external service. Status code: {StatusCode}", response.StatusCode);
        return Result.Failure<TResponse>(ApplicationError.ExternalCallError("External service call failed."));
      }

      var responseContent = await response.Content.ReadAsStringAsync();
      var result = JsonSerializer.Deserialize<TResponse>(responseContent, _options);

      if (result is null)
      {
        _logger.LogError("Failed to deserialize response: {Content}", responseContent);
        return Result.Failure<TResponse>(ApplicationError.ExternalCallError("Failed to deserialize response."));
      }

      _logger.LogDebug("Data posted successfully: {Content}", responseContent);
      return Result.Success(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred while making a POST request to {Endpoint} with data: {Data}", endpoint, data);
      return Result.Failure<TResponse>(ApplicationError.ExternalCallError("An error occurred while making the request."));
    }
  }
}
