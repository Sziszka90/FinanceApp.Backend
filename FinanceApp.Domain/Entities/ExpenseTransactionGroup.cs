using FinanceApp.Domain.Common;

namespace FinanceApp.Domain.Entities;

public class ExpenseTransactionGroup : BaseTransactionGroup
{
  #region Properties

  /// <summary>
  /// Limit on the current expense group
  /// </summary>
  public Money? Limit { get; set; }

  #endregion

  #region Constructors

  /// <inheritdoc />
  public ExpenseTransactionGroup(
    string name,
    string? description,
    string? icon,
    Money? limit,
    User user) : base(name, description, icon, user)
  {
    Limit = limit;
  }

  private ExpenseTransactionGroup() { }

  #endregion

  #region Methods

  public void Update(string name, string? description, string? icon, Money? limit)
  {
    Name = name;
    Description = description;
    Icon = icon;
    Limit = limit;
  }

  #endregion
}
