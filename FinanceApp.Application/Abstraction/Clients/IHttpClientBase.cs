namespace FinanceApp.Application.Abstraction.Clients;

public interface IHttpClientBase
{
  /// <summary>
  /// Sends a GET request to the specified endpoint and returns the response.
  /// </summary>
  Task<TResponse?> GetAsync<TResponse>(string endpoint);

  /// <summary>
  /// Sends a GET request to the specified endpoint with the provided data and returns the response.
  /// </summary>
  Task<TResponse?> GetAsync<TRequest, TResponse>(string endpoint, TRequest data);

  /// <summary>
  /// Sends a POST request to the specified endpoint with the provided data and returns the response.
  /// </summary>
  Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
}
