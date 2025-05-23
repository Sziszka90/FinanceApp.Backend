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

  /// <summary>
  /// User
  /// </summary>
  public User User { get; set; }

  #endregion

  #region Constructors

  /// <inheritdoc />
  protected BaseTransaction(
    string name,
    string? description,
    Money value,
    DateTimeOffset? dueDate,
    BaseTransactionGroup? transactionGroup,
    User user)
  {
    Name = name;
    Description = description;
    Value = value;
    DueDate = dueDate;
    TransactionGroup = transactionGroup;
    User = user;
  }

  protected BaseTransaction() { }

  #endregion
}