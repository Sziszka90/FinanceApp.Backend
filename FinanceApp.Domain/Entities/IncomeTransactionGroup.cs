using FinanceApp.Domain.Common;

namespace FinanceApp.Domain.Entities;

public class IncomeTransactionGroup : BaseTransactionGroup
{
  #region Constructors

  /// <inheritdoc />
  public IncomeTransactionGroup(
    string name,
    string? description,
    string? icon, User user) : base(name, description, icon, user) { }

  private IncomeTransactionGroup() { }

  #endregion

  #region Methods

  public void Update(string name, string? description, string? icon)
  {
    Name = name;
    Description = description;
    Icon = icon;
  }

  #endregion
}