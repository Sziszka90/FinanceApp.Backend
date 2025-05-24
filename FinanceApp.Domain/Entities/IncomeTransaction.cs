using FinanceApp.Domain.Common;

namespace FinanceApp.Domain.Entities;

public class IncomeTransaction : BaseTransaction
{
  /// <inheritdoc />
  public IncomeTransaction(
    string name,
    string? description,
    Money value,
    DateTimeOffset? dueDate,
    IncomeTransactionGroup? group,
    User user) : base(name, description, value, dueDate, group, user) { }

  private IncomeTransaction() { }

  public void Update(
    string name,
    string? description,
    Money value,
    DateTimeOffset? dueDate,
    IncomeTransactionGroup? group)
  {
    Name = name;
    Description = description;
    Value = value;
    DueDate = dueDate;
    TransactionGroup = group;
  }
}
