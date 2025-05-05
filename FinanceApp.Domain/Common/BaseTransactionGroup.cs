namespace FinanceApp.Domain.Common;

public abstract class BaseTransactionGroup : BaseEntity
{
  #region Properties

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
  public string? Icon { get; set; }

  #endregion

  #region Constructors

  protected BaseTransactionGroup(string name, string? description, string? icon)
  {
    Name = name;
    Description = description;
    Icon = icon;
  }

  protected BaseTransactionGroup() { }

  #endregion
}