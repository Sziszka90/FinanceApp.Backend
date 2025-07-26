namespace FinanceApp.Backend.Application.Exceptions;

public class SignalRException : Exception
{
  public string Operation { get; }
  public string? ConnectionId { get; }
  public string? GroupName { get; }
  public string? UserId { get; }

  public SignalRException(string operation, string? connectionId, string message)
    : base(message)
  {
    Operation = operation;
    ConnectionId = connectionId;
    GroupName = null;
    UserId = null;
  }

  public SignalRException(string operation, string? connectionId, string message, Exception innerException)
    : base(message, innerException)
  {
    Operation = operation;
    ConnectionId = connectionId;
    GroupName = null;
    UserId = null;
  }

  public SignalRException(string operation, string? connectionId, string? groupName, string? userId, string message)
    : base(message)
  {
    Operation = operation;
    ConnectionId = connectionId;
    GroupName = groupName;
    UserId = userId;
  }

  public SignalRException(string operation, string? connectionId, string? groupName, string? userId, string message, Exception innerException)
    : base(message, innerException)
  {
    Operation = operation;
    ConnectionId = connectionId;
    GroupName = groupName;
    UserId = userId;
  }
}
