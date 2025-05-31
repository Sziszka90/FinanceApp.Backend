using FinanceApp.Domain.Entities;

namespace FinanceApp.Domain.Interfaces;

public interface IUserOwned
{
  /// <summary>
  /// User which owns the entity
  /// </summary>
  public User User { get; set; }
}
