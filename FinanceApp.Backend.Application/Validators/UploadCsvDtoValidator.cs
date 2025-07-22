using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.Validators;

public class UploadCsvDtoValidator : AbstractValidator<UploadCsvFileDto>
{
  public UploadCsvDtoValidator()
  {
    RuleFor(x => x.File)
      .NotNull()
      .WithMessage("File is required.");

    RuleFor(x => x.File)
      .Must(file => file != null && file.ContentType == "text/csv")
      .WithMessage("Invalid file type. Only CSV files are allowed.");

    RuleFor(x => x.CorrelationId)
      .NotEmpty()
      .WithMessage("CorrelationId is required.");
  }
}
