using FinanceApp.Domain.Interfaces;

namespace FinanceApp.Domain.Common;

public abstract class BaseEntity : IIdentifiable, ITimestampable
{
  #region Properties

  /// <inheritdoc />
  public Guid Id { get; set; }

  /// <inheritdoc />
  public DateTimeOffset Created { get; set; }

  /// <inheritdoc />
  public DateTimeOffset Modified { get; set; }

  #endregion

  #region Constructors

  protected BaseEntity()
  {
    Id = Guid.NewGuid();
    Created = DateTimeOffset.UtcNow;
    Modified = DateTimeOffset.UtcNow;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public void UpdateId(Guid id)
  {
    Id = id;
  }

  /// <inheritdoc />
  public void UpdateModifiedDate()
  {
    Modified = DateTimeOffset.UtcNow;
  }

  #endregion
}