namespace FinanceApp.Backend.Application.Exceptions;

/// <summary>
/// Exception thrown when database operations fail.
/// </summary>
public class DatabaseException : Exception
{
  public string? EntityName { get; }
  public string Operation { get; }
  public string? EntityId { get; }

  public DatabaseException(string operation, string? entityName = null, string? entityId = null)
    : base($"Database operation '{operation}' failed" +
            (entityName != null ? $" for entity '{entityName}'" : "") +
            (entityId != null ? $" with ID '{entityId}'" : ""))
  {
    Operation = operation;
    EntityName = entityName;
    EntityId = entityId;
  }

  public DatabaseException(string operation, string? entityName, string? entityId, Exception innerException)
    : base($"Database operation '{operation}' failed" +
            (entityName != null ? $" for entity '{entityName}'" : "") +
            (entityId != null ? $" with ID '{entityId}'" : ""), innerException)
  {
    Operation = operation;
    EntityName = entityName;
    EntityId = entityId;
  }

  public DatabaseException(string message, Exception innerException) : base(message, innerException)
  {
    Operation = "Unknown";
  }
}
