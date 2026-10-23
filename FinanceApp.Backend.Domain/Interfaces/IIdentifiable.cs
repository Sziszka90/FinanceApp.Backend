namespace FinanceApp.Backend.Domain.Interfaces;

public interface IIdentifiable
{
  /// <summary>
  /// The Primary identifier for the Entity inside the repository
  /// </summary>
  public Guid Id { get; set; }

  /// <summary>
  /// Update the Id of the Entity
  /// </summary>
  /// <param name="id">The new Id to set</param>
  public void UpdateId(Guid id);
}
