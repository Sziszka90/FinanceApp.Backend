using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Domain.Interfaces;

public interface IUserOwned
{
  /// <summary>
  /// User which owns the entity
  /// </summary>
  public User User { get; set; }
}
