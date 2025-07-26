using FinanceApp.Backend.Application.Models;
using FluentValidation;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UploadCsv;

public class LLMProcessorCommandValidator : AbstractValidator<LLMProcessorCommand>
{
  public LLMProcessorCommandValidator()
  {
    RuleFor(x => x.ResponseDto.CorrelationId)
      .NotEmpty()
      .WithMessage(ApplicationError.INVALID_REQUEST_ERROR_MESSAGE);

    RuleFor(x => x.ResponseDto.UserId)
      .NotEmpty()
      .WithMessage(ApplicationError.USER_ID_NOT_PROVIDED_MESSAGE);

    RuleFor(x => x.ResponseDto.Response)
      .NotEmpty()
      .WithMessage(ApplicationError.INVALID_REQUEST_ERROR_MESSAGE);
  }
}
