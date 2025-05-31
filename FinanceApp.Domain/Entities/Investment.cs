using FinanceApp.Domain.Common;
using FinanceApp.Domain.Interfaces;

namespace FinanceApp.Domain.Entities;

public class Investment : BaseEntity, IUserOwned
{
  /// <summary>
  /// Name of the saving
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Value of the saving
  /// </summary>
  public Money Value { get; set; }

  /// <summary>
  /// Description of the investment
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// User which owns the entity
  /// </summary>
  public User User { get; set; }

  /// <inheritdoc />
  public Investment(string name, Money value, string? description, User user)
  {
    Name = name;
    Value = value;
    Description = description;
    User = user;
  }

  private Investment() { }

  public void Update(string name, Money value, string? description)
  {
    Name = name;
    Value = value;
    Description = description;
  }
}
