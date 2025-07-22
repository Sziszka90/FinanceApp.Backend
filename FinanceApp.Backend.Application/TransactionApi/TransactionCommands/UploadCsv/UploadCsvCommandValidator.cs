using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UploadCsv;

public class UploadCsvCommandValidator : AbstractValidator<UploadCsvCommand>
{
  public UploadCsvCommandValidator(IValidator<UploadCsvFileDto> uploadCsvFileDtoValidator)
  {
    RuleFor(x => x.uploadCsvFileDto)
      .SetValidator(uploadCsvFileDtoValidator);
  }
}
