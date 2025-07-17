namespace FinanceApp.Application.Models.Options;

public class LLMProcessorSettings
{
    public required string ApiUrl { get; set; }
    public required string MatchTransactionEndpoint { get; set; }
}
