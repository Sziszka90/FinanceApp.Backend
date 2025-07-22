using FinanceApp.Backend.Domain.Common;
using FinanceApp.Backend.Domain.Interfaces;

namespace FinanceApp.Backend.Domain.Entities;

public class Transaction : BaseEntity, IUserOwned
{
  /// <summary>
  /// Name of the transaction
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Description of the transaction
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Transaction type
  /// </summary>
  public TransactionTypeEnum? TransactionType { get; set; }

  /// <summary>
  /// Value of the transaction
  /// </summary>
  public Money Value { get; set; }

  /// <summary>
  /// Transaction group
  /// </summary>
  public TransactionGroup? TransactionGroup { get; set; }

  /// <summary>
  /// User which owns the entity
  /// </summary>
  public User User { get; set; }

  /// <summary>
  /// Date when Transaction occured
  /// </summary>
  public DateTimeOffset TransactionDate { get; set; }

  public Transaction(
    string name,
    string? description,
    TransactionTypeEnum transactionType,
    Money value,
    TransactionGroup? transactionGroup,
    DateTimeOffset transactionDate,
    User user)
  {
    Name = name;
    Description = description;
    Value = value;
    TransactionGroup = transactionGroup;
    TransactionType = transactionType;
    TransactionDate = transactionDate;
    User = user;
  }

  #pragma warning disable CS8618
  protected Transaction() { }

  public void Update(
    string name,
    string? description,
    Money value,
    TransactionTypeEnum transactionType,
    DateTimeOffset transactionDate,
    TransactionGroup? transactionGroup)
  {
    Name = name;
    Description = description;
    Value = value;
    TransactionGroup = transactionGroup;
    TransactionType = transactionType;
    TransactionDate = transactionDate;
  }
}
