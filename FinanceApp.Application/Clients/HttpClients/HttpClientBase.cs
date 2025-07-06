using System.Text.Json;
using FinanceApp.Application.Abstraction.Clients;

namespace FinanceApp.Application.Clients.HttpClients;

public abstract class HttpClientBase : IHttpClientBase
{
  protected readonly HttpClient _httpClient;

  protected HttpClientBase(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<TResponse?> GetAsync<TResponse>(string endpoint)
  {
    var response = await _httpClient.GetAsync(endpoint);
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<TResponse>(content);
  }

  public async Task<TResponse?> GetAsync<TRequest, TResponse>(string endpoint, TRequest data)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, endpoint)
    {
      Content = new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json")
    };
    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<TResponse>(content);
  }

  public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
  {
    var json = JsonSerializer.Serialize(data);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync(endpoint, content);
    response.EnsureSuccessStatusCode();
    var responseContent = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<TResponse>(responseContent);
  }
}
