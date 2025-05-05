namespace FinanceApp.Domain.Interfaces;

public interface ITimestampable
{
  #region Properties

  /// <summary>
  /// Timestamp of the initial creation of an Entity
  /// </summary>
  public DateTimeOffset Created { get; set; }

  /// <summary>
  /// Timestamp of the update of an Entity
  /// </summary>
  public DateTimeOffset Modified { get; set; }

  #endregion

  #region Methods

  /// <summary>
  /// Update the Modified timestamp to the current time
  /// </summary>
  public void UpdateModifiedDate();

  #endregion
}