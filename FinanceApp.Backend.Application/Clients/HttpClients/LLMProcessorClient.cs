using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Clients.HttpClients;
using FinanceApp.Backend.Application.Dtos.LLMProcessorDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceApp.Backend.Application.Clients;

public class LLMProcessorClient : HttpClientBase<ILLMProcessorClient>, ILLMProcessorClient
{
  private readonly ILogger<ILLMProcessorClient> _logger;
  private readonly LLMProcessorSettings _llmProcessorSettings;

  public LLMProcessorClient(
    ILogger<ILLMProcessorClient> logger,
    HttpClient httpClient,
    IOptions<LLMProcessorSettings> llmProcessorSettings) : base(logger, httpClient)
  {
    _logger = logger;
    _llmProcessorSettings = llmProcessorSettings.Value;
  }

  public async Task<Result<bool>> MatchTransactionGroup(
    string userId,
    List<string> transactionNames,
    List<string> existingGroups,
    string correlationId)
  {
    _ = await PostAsync<MatchTransactionRequestDto, LLMProcessorResponseDto>(_llmProcessorSettings.MatchTransactionEndpoint,
      new MatchTransactionRequestDto
      {
        UserId = userId,
        TransactionNames = transactionNames,
        TransactionGroupNames = existingGroups,
        CorrelationId = correlationId
      });

    return Result.Success(true);
  }
}
