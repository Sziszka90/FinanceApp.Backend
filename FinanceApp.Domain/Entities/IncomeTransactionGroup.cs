using FinanceApp.Domain.Common;

namespace FinanceApp.Domain.Entities;

public class IncomeTransactionGroup : BaseTransactionGroup
{
  /// <inheritdoc />
  public IncomeTransactionGroup(
    string name,
    string? description,
    string? icon, User user) : base(name, description, icon, user) { }

  private IncomeTransactionGroup() { }

  public void Update(string name, string? description, string? icon)
  {
    Name = name;
    Description = description;
    Icon = icon;
  }
}
