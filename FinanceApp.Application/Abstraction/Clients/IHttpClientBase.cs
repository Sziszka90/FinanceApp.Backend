namespace FinanceApp.Application.Abstraction.Clients;

public interface IHttpClientBase
{
  /// <summary>
  /// Sends a GET request to the specified endpoint and returns the response.
  /// </summary>
  /// <typeparam name="TResponse">The type of the response.</typeparam>
  /// <param name="endpoint">The endpoint to send the request to.</param>
  /// <returns>The response from the GET request.</returns>
  Task<TResponse?> GetAsync<TResponse>(string endpoint);

  /// <summary>
  /// Sends a GET request to the specified endpoint with the provided data and returns the response.
  /// </summary>
  /// <typeparam name="TRequest">The type of the request data.</typeparam>
  /// <typeparam name="TResponse">The type of the response.</typeparam>
  /// <param name="endpoint">The endpoint to send the request to.</param>
  /// <param name="data">The data to include in the GET request.</param>
  /// <returns>The response from the GET request.</returns>
  Task<TResponse?> GetAsync<TRequest, TResponse>(string endpoint, TRequest data);

  /// <summary>
  /// Sends a POST request to the specified endpoint with the provided data and returns the response.
  /// </summary>
  /// <typeparam name="TRequest">The type of the request data.</typeparam>
  /// <typeparam name="TResponse">The type of the response.</typeparam>
  /// <param name="endpoint">The endpoint to send the request to.</param>
  /// <param name="data">The data to include in the POST request.</param>
  /// <returns>The response from the POST request.</returns>
  Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
}
