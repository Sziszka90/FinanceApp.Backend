using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;

namespace FinanceApp.Backend.Application.Models;

public class TransactionGroupAggregate
{
  public required TransactionGroup TransactionGroup { get; set; }
  public CurrencyEnum Currency { get; set; }
  public decimal TotalAmount { get; set; }
  public int TransactionCount { get; set; }
}
