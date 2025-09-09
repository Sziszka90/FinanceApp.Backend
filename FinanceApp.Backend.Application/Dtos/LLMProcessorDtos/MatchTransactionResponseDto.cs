namespace FinanceApp.Backend.Application.Dtos.RabbitMQDtos;

public class MatchTransactionResponseDto
{
  public required Dictionary<string, string> Transactions { get; set; }
}
