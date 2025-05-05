using FinanceApp.Domain.Common;

namespace FinanceApp.Domain.Entities;

public class ExpenseTransaction : BaseTransaction
{
  #region Properties

  /// <summary>
  /// Priority of the given expense
  /// </summary>
  public int? Priority { get; set; }

  #endregion

  #region Constructors

  /// <inheritdoc />
  public ExpenseTransaction(
    string name,
    string? description,
    Money value,
    DateTimeOffset? dueDate,
    ExpenseTransactionGroup? transactionGroup,
    int? priority) :
    base(name, description, value, dueDate, transactionGroup)
  {
    Priority = priority;
  }

  private ExpenseTransaction() { }

  #endregion

  #region Methods

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

  #endregion
}