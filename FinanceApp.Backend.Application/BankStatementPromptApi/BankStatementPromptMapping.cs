using FinanceApp.Backend.Application.Dtos.BankStatementPromptDtos;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.BankStatementPromptApi;

internal static class BankStatementPromptMapping
{
  public static BankStatementPromptDto ToDto(BankStatementPrompt prompt) => new()
  {
    Id = prompt.Id,
    Name = prompt.Name,
    Instructions = prompt.Instructions,
    IsSelected = prompt.IsSelected
  };
}
