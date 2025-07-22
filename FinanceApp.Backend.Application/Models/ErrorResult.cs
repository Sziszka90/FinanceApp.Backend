namespace FinanceApp.Backend.Application.Models;

public class ErrorResult(ApplicationError error, string path)
{
  /// <summary>
  /// Represents an error result with details about the error.
  /// </summary>
  public string Message { get; } = error.Message;

  /// <summary>
  /// A dictionary containing additional details about the error.
  /// </summary>
  public Dictionary<string, object> Details { get; set; } = error.Details;

  /// <summary>
  /// The path where the error occurred.
  /// </summary>
  public string Path { get; } = path;

  /// <summary>
  /// The error code associated with the error.
  /// </summary>
  public string Code { get; } = error.Code;
}
