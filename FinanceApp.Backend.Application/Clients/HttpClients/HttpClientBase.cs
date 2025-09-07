using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.Clients.HttpClients;

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

  public async Task<TResponse> GetAsync<TResponse>(string endpoint)
  {
    try
    {
      var response = await _httpClient.GetAsync(endpoint);

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("Failed to fetch data from external service. Status code: {StatusCode}", response.StatusCode);
        throw new HttpClientException("GET", endpoint, (int)response.StatusCode, "External service call failed.");
      }

      var content = await response.Content.ReadAsStringAsync();
      var result = JsonSerializer.Deserialize<TResponse>(content, _options);

      if (result is null)
      {
        _logger.LogError("Failed to deserialize response: {Content}", content);
        throw new HttpClientException("GET_DESERIALIZE", endpoint, "Failed to deserialize response.");
      }

      _logger.LogDebug("Data fetched successfully: {Content}", content);
      return result;
    }
    catch (HttpClientException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred while making a GET request to {Endpoint}", endpoint);
      throw new HttpClientException("GET", endpoint, "An error occurred while making the request.", ex);
    }
  }

  public async Task<TResponse> GetAsync<TRequest, TResponse>(string endpoint, TRequest data)
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
        throw new HttpClientException("GET_WITH_DATA", endpoint, (int)response.StatusCode, "External service call failed.");
      }

      var content = await response.Content.ReadAsStringAsync();
      var result = JsonSerializer.Deserialize<TResponse>(content, _options);

      if (result is null)
      {
        _logger.LogError("Failed to deserialize response: {Content}", content);
        throw new HttpClientException("GET_WITH_DATA_DESERIALIZE", endpoint, "Failed to deserialize response.");
      }

      _logger.LogDebug("Data fetched successfully: {Content}", content);
      return result;
    }
    catch (HttpClientException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred while making a GET request to {Endpoint} with data: {Data}", endpoint, data);
      throw new HttpClientException("GET_WITH_DATA", endpoint, "An error occurred while making the request.", ex);
    }
  }

  public async Task PostAsync(string endpoint)
  {
    var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

    try
    {
      var response = await _httpClient.PostAsync(endpoint, content);

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("Failed to post data to external service. Status code: {StatusCode}", response.StatusCode);
        throw new HttpClientException("POST", endpoint, (int)response.StatusCode, "External service call failed.");
      }
    }
    catch (HttpClientException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred while making a POST request to {Endpoint}", endpoint);
      throw new HttpClientException("POST", endpoint, "An error occurred while making the request.", ex);
    }
  }

  public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
  {
    var json = JsonSerializer.Serialize(data);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

    try
    {
      var response = await _httpClient.PostAsync(endpoint, content);

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("Failed to post data to external service. Status code: {StatusCode}", response.StatusCode);
        throw new HttpClientException("POST", endpoint, (int)response.StatusCode, "External service call failed.");
      }

      var responseContent = await response.Content.ReadAsStringAsync();
      var result = JsonSerializer.Deserialize<TResponse>(responseContent, _options);

      if (result is null)
      {
        _logger.LogError("Failed to deserialize response: {Content}", responseContent);
        throw new HttpClientException("POST_DESERIALIZE", endpoint, "Failed to deserialize response.");
      }

      _logger.LogDebug("Data posted successfully: {Content}", responseContent);
      return result;
    }
    catch (HttpClientException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An error occurred while making a POST request to {Endpoint} with data: {Data}", endpoint, data);
      throw new HttpClientException("POST", endpoint, "An error occurred while making the request.", ex);
    }
  }
}
