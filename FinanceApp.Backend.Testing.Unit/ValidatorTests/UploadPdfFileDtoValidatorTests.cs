using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.ImportPdf;
using FinanceApp.Backend.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class UploadPdfFileDtoValidatorTests
{
  private readonly UploadPdfFileDtoValidator _validator = new();

  [Fact]
  public void Validate_ValidPdfAndCorrelationId_IsValid()
  {
    var result = _validator.Validate(CreateDto("statement.PDF", 1024, Guid.NewGuid().ToString()));

    Assert.True(result.IsValid);
  }

  [Fact]
  public void Validate_MissingFile_IsInvalid()
  {
    var result = _validator.Validate(new UploadPdfFileDto { File = null!, CorrelationId = Guid.NewGuid().ToString() });

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, error => error.PropertyName == "File");
  }

  [Theory]
  [InlineData("statement.csv", 1024, "valid-guid")]
  [InlineData("statement.pdf", 0, "valid-guid")]
  [InlineData("statement.pdf", 10485761, "valid-guid")]
  [InlineData("statement.pdf", 1024, "not-a-guid")]
  public void Validate_InvalidFileOrCorrelationId_IsInvalid(string fileName, long length, string correlationId)
  {
    var result = _validator.Validate(CreateDto(fileName, length, correlationId));

    Assert.False(result.IsValid);
  }

  [Fact]
  public void ImportPdfCommandValidator_UsesUploadValidator()
  {
    IValidator<UploadPdfFileDto> uploadValidator = _validator;
    var validator = new ImportPdfCommandValidator(uploadValidator);
    var command = new ImportPdfCommand(CreateDto("statement.pdf", 512, Guid.NewGuid().ToString()));

    Assert.True(validator.Validate(command).IsValid);
    Assert.False(validator.Validate(new ImportPdfCommand(CreateDto("statement.txt", 512, Guid.NewGuid().ToString()))).IsValid);
  }

  private static UploadPdfFileDto CreateDto(string fileName, long length, string correlationId)
  {
    var file = new Mock<IFormFile>();
    file.SetupGet(item => item.FileName).Returns(fileName);
    file.SetupGet(item => item.Length).Returns(length);
    return new UploadPdfFileDto { File = file.Object, CorrelationId = correlationId };
  }
}
