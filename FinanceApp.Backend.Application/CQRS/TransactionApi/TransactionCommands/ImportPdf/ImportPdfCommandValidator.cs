using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.ImportPdf;

public class ImportPdfCommandValidator : AbstractValidator<ImportPdfCommand>
{
  public ImportPdfCommandValidator(IValidator<UploadPdfFileDto> validator)
  {
    RuleFor(x => x.Upload).SetValidator(validator);
  }
}
