using FinanceApp.Application.Models;
using FluentValidation;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.UploadCsv;

public class UploadCsvCommandValidator : AbstractValidator<UploadCsvCommand>
{
  public UploadCsvCommandValidator()
  {
    RuleFor(x => x.uploadCsvFileDto.File)
      .NotNull()
      .Must(file => file != null && file.Length > 0)
      .WithMessage(ApplicationError.FILE_EMPTY_ERROR_MESSAGE)
      .Must(file => file != null && file.ContentType == "text/csv")
      .WithMessage(ApplicationError.INVALID_FILE_TYPE_ERROR_MESSAGE);
  }
}
