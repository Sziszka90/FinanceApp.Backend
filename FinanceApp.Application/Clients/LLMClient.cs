using System.Text.Json;
using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
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

  public async Task<Result<List<Domain.Entities.TransactionGroup>>> CreateTransactionGroup(List<string> transactionNames, Domain.Entities.User user, CancellationToken cancellationToken = default)
  {
    var systemPrompt = """
      You are a financial assistant creating transaction groups for bank transactions.
      Your task is to analyse the provided transaction names and categorise them into appropriate groups based on their nature.
      Such as salary, groceries, utilities, car, home, travel, food, electronics, entertainment, etc.
      Group name and description should be general and not specific to any bank, store or country.
      For example, if the transaction name is "Groceries", the description should be "Payment for groceries".
      If the transaction name is "Entertainment", the description should be "Expense for entertainment".
      You should return the same length of transaction groups as the number of transaction names provided.
      The JSON should look like this:
      [
      { "name": "Groceries", "description": "Payment for groceries" }
      { "name": "Entertainment", "description": "Expense for entertainment" }
      { "name": "Car", "description": "Car cost and fuel" }
      { "name": "Transport", "description": "Transport costs like ticket or pass" }
      { "name": "Food", "description": "Canteen or Restaurant payment" }
      ]
      Only return the JSON response in a list without any additional text or explanation. Without line breaks or markdown code blocks.
      """;

    var transactionNamesPrompt = $"Return transaction groups for the following transactions: {string.Join("; ", transactionNames)}. They are divided by ;";

    ChatCompletion completion = await _client.CompleteChatAsync(systemPrompt + transactionNamesPrompt);

    var responseText = completion.Content[0].Text;

    JsonSerializerOptions options = new()
    {
      PropertyNameCaseInsensitive = true,
      WriteIndented = false
    };

    var transactionGroups = JsonSerializer.Deserialize<List<GetTransactionGroupDto>>(responseText, options);

    if (transactionGroups == null)
    {
      _logger.LogError("Failed to deserialize transaction group response: {ResponseText}", responseText);
      return Result.Failure<List<Domain.Entities.TransactionGroup>>(ApplicationError.ExternalCallError("Failed to parse transaction group response from LLM."));
    }

    var transactionGroupEntities = transactionGroups.Select(tg => new Domain.Entities.TransactionGroup(tg.Name, tg.Description, null, user)).ToList();

    return Result.Success(transactionGroupEntities);
  }
}
