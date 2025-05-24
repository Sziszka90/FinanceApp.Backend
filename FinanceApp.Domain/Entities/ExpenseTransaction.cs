using FinanceApp.Domain.Common;

namespace FinanceApp.Domain.Entities;

public class ExpenseTransaction : BaseTransaction
{
  /// <summary>
  /// Priority of the given expense
  /// </summary>
  public int? Priority { get; set; }

  /// <inheritdoc />
  public ExpenseTransaction(
    string name,
    string? description,
    Money value,
    DateTimeOffset? dueDate,
    ExpenseTransactionGroup? transactionGroup,
    int? priority,
    User user) :
    base(name, description, value, dueDate, transactionGroup, user)
  {
    Priority = priority;
  }

  private ExpenseTransaction() { }

  public void Update(
    string name,
    string? description,
    Money value,
    DateTimeOffset? dueDate,
    ExpenseTransactionGroup? transactionGroup,
    int? priority)
  {
    Name = name;
    Description = description;
    Value = value;
    DueDate = dueDate;
    TransactionGroup = transactionGroup;
    Priority = priority;
  }
}
