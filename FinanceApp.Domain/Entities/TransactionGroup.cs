using FinanceApp.Domain.Common;
using FinanceApp.Domain.Interfaces;

namespace FinanceApp.Domain.Entities;

public class TransactionGroup : BaseEntity, IUserOwned
{
  /// <summary>
  /// Name of the transaction group
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Description of the transaction Group
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Icon of the transaction group
  /// </summary>
  public string? GroupIcon { get; set; }

  /// <summary>
  /// User which owns the entity
  /// </summary>
  public User User { get; set; }

  public TransactionGroup(
    string name,
    string? description,
    string? groupIcon,
    User user)
  {
    Name = name;
    Description = description;
    GroupIcon = groupIcon;
    User = user;
  }

  #pragma warning disable CS8618
  protected TransactionGroup() { }

  public void Update(
    string name,
    string? description,
    string? groupIcon)
  {
    Name = name;
    Description = description;
    GroupIcon = groupIcon;
  }
}
