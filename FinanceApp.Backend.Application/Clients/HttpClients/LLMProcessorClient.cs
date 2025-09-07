using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Clients.HttpClients;
using FinanceApp.Backend.Application.Dtos.LLMProcessorDtos;
using FinanceApp.Backend.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.Clients;

public class LLMProcessorClient : HttpClientBase<ILLMProcessorClient>, ILLMProcessorClient
{
  public LLMProcessorClient(
    ILogger<ILLMProcessorClient> logger,
    HttpClient httpClient) : base(logger, httpClient)
  { }

  public async Task<Result<bool>> MatchTransactionGroup(
    string userId,
    List<string> transactionNames,
    List<string> existingGroups,
    string correlationId)
  {
    _ = await PostAsync<MatchTransactionRequestDto, LLMProcessorResponseDto>("/llmprocessor/match-transactions",
      new MatchTransactionRequestDto
      {
        UserId = userId,
        TransactionNames = transactionNames,
        TransactionGroupNames = existingGroups,
        CorrelationId = correlationId
      });

    return Result.Success(true);
  }

  public async Task<Result<bool>> WakeupAsync()
  {
    await PostAsync("/wakeup");
    return Result.Success(true);
  }
}
