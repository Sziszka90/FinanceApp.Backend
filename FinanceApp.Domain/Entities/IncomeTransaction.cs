using FinanceApp.Domain.Common;

namespace FinanceApp.Domain.Entities;

public class IncomeTransaction : BaseTransaction
{
  #region Constructors

  /// <inheritdoc />
  public IncomeTransaction(
    string name,
    string? description,
    Money value,
    DateTimeOffset? dueDate,
    IncomeTransactionGroup? group) : base(name, description, value, dueDate, group) { }

  private IncomeTransaction() { }

  #endregion

  #region Methods

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

  #endregion
}