using FinanceApp.Domain.Common;

namespace FinanceApp.Domain.Entities;

public class Investment : BaseEntity
{
  #region Properties

  /// <summary>
  /// Name of the saving
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Amount of the saving
  /// </summary>
  public Money Amount { get; set; }

  /// <summary>
  /// Description of the investment
  /// </summary>
  public string? Description { get; set; }

  #endregion

  #region Constructors

  /// <inheritdoc />
  public Investment(string name, Money amount, string? description)
  {
    Name = name;
    Amount = amount;
    Description = description;
  }

  private Investment() { }

  #endregion

  #region Methods

  public void Update(string name, Money amount, string? description)
  {
    Name = name;
    Amount = amount;
    Description = description;
  }

  #endregion
}
