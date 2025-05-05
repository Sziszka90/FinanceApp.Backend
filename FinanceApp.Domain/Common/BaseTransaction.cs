using FinanceApp.Domain.Entities;

namespace FinanceApp.Domain.Common;

public abstract class BaseTransaction : BaseEntity
{
  #region Properties

  /// <summary>
  /// Name of the transaction
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Description of the transaction
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Value of the transaction
  /// </summary>
  public Money Value { get; set; }

  /// <summary>
  /// Due date of the transaction
  /// </summary>
  public DateTimeOffset? DueDate { get; set; }

  /// <summary>
  /// Transaction group
  /// </summary>
  public BaseTransactionGroup? TransactionGroup { get; set; }

  #endregion

  #region Constructors

  /// <inheritdoc />
  protected BaseTransaction(
    string name,
    string? description,
    Money value,
    DateTimeOffset? dueDate,
    BaseTransactionGroup? transactionGroup)
  {
    Name = name;
    Description = description;
    Value = value;
    DueDate = dueDate;
    TransactionGroup = transactionGroup;
  }

  protected BaseTransaction() { }

  #endregion
}