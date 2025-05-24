namespace FinanceApp.Domain.Interfaces;

public interface IIdentifiable
{
  #region Properties

  /// <summary>
  /// The Primary identifier for the Entity inside the repository
  /// </summary>
  public Guid Id { get; set; }

  #endregion

  #region Methods

  /// <summary>
  /// Update the Id of the Entity
  /// </summary>
  /// <param name="id"></param>
  public void UpdateId(Guid id);

  #endregion
}
