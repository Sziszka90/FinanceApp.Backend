using FinanceApp.Domain.Common;

namespace FinanceApp.Domain.Entities;

public class Saving : BaseEntity
{
  #region Properties

  /// <summary>
  /// Name of the saving
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Description of the investment
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Amount of the saving
  /// </summary>
  public Money Amount { get; set; }

  /// <summary>
  /// Type of the Saving
  /// </summary>
  public SavingTypeEnum Type { get; set; }

  /// <summary>
  /// Date until the saving is valid
  /// </summary>
  public DateTimeOffset? DueDate { get; set; }

  #endregion

  #region Constructors

  /// <inheritdoc />
  public Saving(string name, string? description, Money amount, SavingTypeEnum type, DateTimeOffset? dueDate)
  {
    Name = name;
    Description = description;
    Amount = amount;
    Type = type;
    DueDate = dueDate;
  }

  private Saving() { }

  #endregion

  #region Methods

  public void Update(string name, string? description, Money amount, SavingTypeEnum type, DateTimeOffset? dueDate)
  {
    Name = name;
    Description = description;
    Amount = amount;
    Type = type;
    DueDate = dueDate;
  }

  #endregion
}