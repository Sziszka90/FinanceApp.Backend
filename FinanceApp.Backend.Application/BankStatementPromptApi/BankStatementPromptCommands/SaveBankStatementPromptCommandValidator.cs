using FinanceApp.Backend.Application.Dtos.BankStatementPromptDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.BankStatementPromptApi.BankStatementPromptCommands;

public class SaveBankStatementPromptCommandValidator : AbstractValidator<SaveBankStatementPromptCommand>
{
  public SaveBankStatementPromptCommandValidator(IValidator<SaveBankStatementPromptDto> promptValidator)
  {
    RuleFor(command => command.Prompt).SetValidator(promptValidator);
  }
}
