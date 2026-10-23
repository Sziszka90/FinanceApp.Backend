namespace FinanceApp.Backend.Application.Exceptions;

public class HttpClientException : Exception
{
  public string Operation { get; }
  public string? Endpoint { get; }
  public int? StatusCode { get; }

  public HttpClientException(string operation, string? endpoint, int? statusCode, string message)
    : base(message)
  {
    Operation = operation;
    Endpoint = endpoint;
    StatusCode = statusCode;
  }

  public HttpClientException(string operation, string? endpoint, int? statusCode, string message, Exception innerException)
    : base(message, innerException)
  {
    Operation = operation;
    Endpoint = endpoint;
    StatusCode = statusCode;
  }

  public HttpClientException(string operation, string? endpoint, string message)
    : base(message)
  {
    Operation = operation;
    Endpoint = endpoint;
    StatusCode = null;
  }

  public HttpClientException(string operation, string? endpoint, string message, Exception innerException)
    : base(message, innerException)
  {
    Operation = operation;
    Endpoint = endpoint;
    StatusCode = null;
  }
}
