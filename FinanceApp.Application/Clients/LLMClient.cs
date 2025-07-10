using System.Text.Json;
using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace FinanceApp.Application.Clients;

public class LLMClient : ILLMClient
{
  private readonly ILogger<LLMClient> _logger;
  private readonly ChatClient _client;
  private readonly ITransactionGroupRepository _transactionGroupRepository;

  public LLMClient(
    ChatClient client,
    ILogger<LLMClient> logger,
    ITransactionGroupRepository transactionGroupRepository)
  {
    _client = client;
    _logger = logger;
    _transactionGroupRepository = transactionGroupRepository;
  }

  public async Task<Result<List<Dictionary<string, string>>>> CreateTransactionGroup(List<string> transactionNames, List<string> existingGroups, Domain.Entities.User user, CancellationToken cancellationToken = default)
  {
    var prompt = """
      You are a financial assistant creating transaction groups for bank transactions.
      Your task is to analyse the provided transaction names and categorise them into appropriate groups based on their nature.
      Such as salary, groceries, utilities, car, home, travel, food, electronics, entertainment, etc.
      I will provide you with a list of available transaction groups, and you should match the most suitable group for each transaction name.
      """ +
      $"Transaction groups: {string.Join(", ", existingGroups)}." +
      """
      Return the transaction groups in list with the following structure:
      [{"Transaction Name 1" : "Group Name 1"}, {"Transaction Name 2" : "Group Name 2"}, ...]
      Only return the JSON response in a list without any additional text or explanation. Without line breaks or markdown code blocks.
      """ +
      $"Return transaction groups for the following transactions: {string.Join("; ", transactionNames)}. They are divided by ;";

    ChatCompletion completion = await _client.CompleteChatAsync(prompt);

    var responseText = completion.Content[0].Text;

    JsonSerializerOptions options = new()
    {
      PropertyNameCaseInsensitive = true,
      WriteIndented = false
    };

    var result = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(responseText, options);

    if (result == null)
    {
      _logger.LogError("Failed to deserialize transaction group response: {ResponseText}", responseText);
      return Result.Failure<List<Dictionary<string, string>>>(ApplicationError.ExternalCallError("Failed to parse transaction group response from LLM."));
    }
    return Result.Success(result);
  }
}
