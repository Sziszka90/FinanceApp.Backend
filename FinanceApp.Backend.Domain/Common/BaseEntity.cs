using FinanceApp.Backend.Domain.Interfaces;

namespace FinanceApp.Backend.Domain.Common;

public abstract class BaseEntity : IIdentifiable, ITimestampable
{
  /// <inheritdoc />
  public Guid Id { get; set; }

  /// <inheritdoc />
  public DateTimeOffset Created { get; set; }

  /// <inheritdoc />
  public DateTimeOffset Modified { get; set; }

  protected BaseEntity()
  {
    Id = Guid.NewGuid();
    Created = DateTimeOffset.UtcNow;
    Modified = DateTimeOffset.UtcNow;
  }

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
}
