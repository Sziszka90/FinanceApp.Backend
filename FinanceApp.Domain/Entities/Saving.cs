using FinanceApp.Domain.Common;
using FinanceApp.Domain.Interfaces;

namespace FinanceApp.Domain.Entities;

public class Saving : BaseEntity, IUserOwned
{
  /// <summary>
  /// Name of the saving
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Description of the investment
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Value of the saving
  /// </summary>
  public Money Value { get; set; }

  /// <summary>
  /// Type of the Saving
  /// </summary>
  public SavingTypeEnum Type { get; set; }

  /// <summary>
  /// Date until the saving is valid
  /// </summary>
  public DateTimeOffset? DueDate { get; set; }

  /// <summary>
  /// User which owns the entity
  /// </summary>
  public User User { get; set; }

  /// <inheritdoc />
  public Saving(string name, string? description, Money value, SavingTypeEnum type, DateTimeOffset? dueDate, User user)
  {
    Name = name;
    Description = description;
    Value = value;
    Type = type;
    DueDate = dueDate;
    User = user;
  }

  private Saving() { }

  public void Update(string name, string? description, Money value, SavingTypeEnum type, DateTimeOffset? dueDate)
  {
    Name = name;
    Description = description;
    Value = value;
    Type = type;
    DueDate = dueDate;
  }
}
