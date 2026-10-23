using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.Validators;

public class UploadPdfFileDtoValidator : AbstractValidator<UploadPdfFileDto>
{
  public const long MaximumFileSize = 10 * 1024 * 1024;

  public UploadPdfFileDtoValidator()
  {
    RuleFor(x => x.File).NotNull();
    RuleFor(x => x.File).Must(file => file is { Length: > 0 and <= MaximumFileSize })
      .WithMessage("PDF must be between 1 byte and 10 MB.");
    RuleFor(x => x.File).Must(file => file is not null &&
        string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
      .WithMessage("Only PDF files are supported.");
    RuleFor(x => x.CorrelationId).Must(value => Guid.TryParse(value, out _))
      .WithMessage("A valid correlation ID is required.");
  }
}
