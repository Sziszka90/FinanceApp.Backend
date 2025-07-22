namespace FinanceApp.Backend.Domain.Options;

public class LLMProcessorSettings
{
  public required string ApiUrl { get; set; }
  public required string MatchTransactionEndpoint { get; set; }
  public required string Token { get; set; }
}
