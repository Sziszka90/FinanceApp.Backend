using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Dtos.LLMProcessorDtos;
using FinanceApp.Backend.Application.Clients.HttpClients;
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
    List<string> transactionNames,
    List<string> existingGroups,
    string userId,
    string correlationId)
  {
    var response = await PostAsync<LLMProcessorRequestDto, LLMProcessorResponseDto>(_llmProcessorSettings.MatchTransactionEndpoint,
      new LLMProcessorRequestDto
      {
        UserId = userId,
        TransactionNames = transactionNames,
        TransactionGroupNames = existingGroups,
        CorrelationId = correlationId
      });

    if (response.IsSuccess)
    {
      return Result.Success(true);
    }

    _logger.LogError("Failed request to LLM Processor Service: {ErrorMessage}", response);
    return Result.Failure<bool>(response.ApplicationError!);
  }
}
