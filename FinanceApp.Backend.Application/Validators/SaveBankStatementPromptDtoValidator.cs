using FinanceApp.Backend.Application.Dtos.BankStatementPromptDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.Validators;

public class SaveBankStatementPromptDtoValidator : AbstractValidator<SaveBankStatementPromptDto>
{
  public SaveBankStatementPromptDtoValidator()
  {
    RuleFor(prompt => prompt.Name).NotEmpty().MaximumLength(100);
    RuleFor(prompt => prompt.Instructions).NotEmpty().MaximumLength(4000);
  }
}
