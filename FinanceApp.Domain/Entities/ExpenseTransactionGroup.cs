using FinanceApp.Domain.Common;

namespace FinanceApp.Domain.Entities;

public class ExpenseTransactionGroup : BaseTransactionGroup
{
  /// <summary>
  /// Limit on the current expense group
  /// </summary>
  public Money? Limit { get; set; }

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

  public void Update(string name, string? description, string? icon, Money? limit)
  {
    Name = name;
    Description = description;
    Icon = icon;
    Limit = limit;
  }
}
