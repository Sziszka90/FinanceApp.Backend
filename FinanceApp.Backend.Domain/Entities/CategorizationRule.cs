using FinanceApp.Backend.Domain.Common;
using FinanceApp.Backend.Domain.Interfaces;

namespace FinanceApp.Backend.Domain.Entities;

public class CategorizationRule : BaseEntity, IUserOwned
{
  public Guid UserId { get; set; }
  public User User { get; set; } = null!;
  public string MerchantText { get; set; } = string.Empty;
  public Guid TransactionGroupId { get; set; }
  public TransactionGroup TransactionGroup { get; set; } = null!;

  public void Update(string merchantText, Guid transactionGroupId)
  {
    MerchantText = merchantText;
    TransactionGroupId = transactionGroupId;
  }
}
