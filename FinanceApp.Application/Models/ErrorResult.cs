namespace FinanceApp.Application.Models;

public class ErrorResult(ApplicationError error, string path)
{
  public string Message { get; } = error.Message;
  public Dictionary<string, object> Details { get; set; } = error.Details;
  public string Path { get; } = path;
  public string Code { get; } = error.Code;
}
