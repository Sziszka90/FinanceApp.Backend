using FinanceApp.Application.Dtos;
using FluentValidation;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class CreateIncomeCommandValidator : AbstractValidator<CreateIncomeCommand>
{
  #region Constructors

  public CreateIncomeCommandValidator(IValidator<CreateIncomeTransactionDto> createIncomeTransactionDtoValidator)
  {
    RuleFor(x => x.CreateIncomeTransactionDto)
      .SetValidator(createIncomeTransactionDtoValidator);
  }

  #endregion
}