using FinanceApp.Backend.Domain.Common;
using FinanceApp.Backend.Domain.Interfaces;

namespace FinanceApp.Backend.Domain.Entities;

public class BankStatementPrompt : BaseEntity, IUserOwned
{
  public Guid UserId { get; set; }
  public User User { get; set; } = null!;
  public string Name { get; set; } = string.Empty;
  public string Instructions { get; set; } = string.Empty;
  public bool IsSelected { get; set; }

  public void Update(string name, string instructions)
  {
    Name = name;
    Instructions = instructions;
  }
}
