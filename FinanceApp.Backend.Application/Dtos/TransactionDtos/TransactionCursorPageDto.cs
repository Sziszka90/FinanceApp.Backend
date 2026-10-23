namespace FinanceApp.Backend.Application.Dtos.TransactionDtos;

public class TransactionCursorPageDto
{
  public List<GetTransactionDto> Items { get; set; } = [];
  public string? NextCursor { get; set; }
}
