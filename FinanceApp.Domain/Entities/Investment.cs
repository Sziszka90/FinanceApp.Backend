using FinanceApp.Domain.Common;

namespace FinanceApp.Domain.Entities;

public class Investment : BaseEntity
{
  /// <summary>
  /// Name of the saving
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Value of the saving
  /// </summary>
  public Money Value { get; set; }

  /// <summary>
  /// Description of the investment
  /// </summary>
  public string? Description { get; set; }

  /// <inheritdoc />
  public Investment(string name, Money value, string? description)
  {
    Name = name;
    Value = value;
    Description = description;
  }

  private Investment() { }

  public void Update(string name, Money value, string? description)
  {
    Name = name;
    Value = value;
    Description = description;
  }
}
