namespace FinanceApp.Backend.Domain.Interfaces;

public interface ITimestampable
{
  /// <summary>
  /// Timestamp of the initial creation of an Entity
  /// </summary>
  public DateTimeOffset Created { get; set; }

  /// <summary>
  /// Timestamp of the update of an Entity
  /// </summary>
  public DateTimeOffset Modified { get; set; }

  /// <summary>
  /// Update the Modified timestamp to the current time
  /// </summary>
  public void UpdateModifiedDate();
}
