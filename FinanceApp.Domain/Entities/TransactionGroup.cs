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
  public Icon? GroupIcon { get; set; }

  /// <summary>
  /// Limit on the current expense group
  /// </summary>
  public Money? Limit { get; set; }

  /// <summary>
  /// User which owns the entity
  /// </summary>
  public User User { get; set; }

  public TransactionGroup(
    string name,
    string? description,
    Icon? groupIcon,
    User user,
    Money? limit)
  {
    Name = name;
    Description = description;

    if (groupIcon is not null)
    {
      GroupIcon = new Icon(
        groupIcon.FileName,
        groupIcon.ContentType,
        groupIcon.Data
      );
    }
    Limit = limit;
    User = user;
  }

  protected TransactionGroup() { }

  public void Update(
    string name,
    string? description,
    Icon? groupIcon,
    Money? limit)
  {
    Name = name;
    Description = description;

    if (groupIcon is not null)
    {
      if (GroupIcon is not null)
      {
        GroupIcon!.Update(
          groupIcon.FileName,
          groupIcon.ContentType,
          groupIcon.Data
        );
      }
      else
      {
        GroupIcon = new Icon(
          groupIcon.FileName,
          groupIcon.ContentType,
          groupIcon.Data
        );
      }
    }
    else
    {
      GroupIcon = null;
    }
    Limit = limit;
  }
}
