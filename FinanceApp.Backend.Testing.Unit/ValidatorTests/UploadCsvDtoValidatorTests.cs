using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Validators;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class UploadCsvDtoValidatorTests : ValidatorTestBase
{
  private readonly UploadCsvDtoValidator _validator;

  public UploadCsvDtoValidatorTests()
  {
    _validator = new UploadCsvDtoValidator();
  }

  public class ValidDtoTests : UploadCsvDtoValidatorTests
  {
    [Fact]
    public void ValidDto_ShouldNotHaveValidationErrors()
    {
      // arrange
      var mockFile = CreateMockFile("test.csv", "text/csv", "name,amount\nTest,100");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = Guid.NewGuid().ToString()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class FileValidationTests : UploadCsvDtoValidatorTests
  {
    [Fact]
    public void File_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var dto = new UploadCsvFileDto
      {
        File = null!,
        CorrelationId = Guid.NewGuid().ToString()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.File)
        .WithErrorMessage("File is required.");
    }

    [Fact]
    public void File_WhenValidCsvFile_ShouldNotHaveValidationError()
    {
      // arrange
      var mockFile = CreateMockFile("transactions.csv", "text/csv", "name,amount\nTest,100");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = Guid.NewGuid().ToString()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.File);
    }

    [Theory]
    [InlineData("application/vnd.ms-excel")]
    [InlineData("text/plain")]
    [InlineData("application/json")]
    [InlineData("text/xml")]
    [InlineData("image/jpeg")]
    [InlineData("application/pdf")]
    public void File_WhenInvalidContentType_ShouldHaveValidationError(string contentType)
    {
      // arrange
      var mockFile = CreateMockFile("test.csv", contentType, "content");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = Guid.NewGuid().ToString()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.File)
        .WithErrorMessage("Invalid file type. Only CSV files are allowed.");
    }

    [Theory]
    [InlineData("text/csv")]
    [InlineData("TEXT/CSV")]
    [InlineData("Text/Csv")]
    public void File_WhenValidContentTypeWithDifferentCase_ShouldNotHaveValidationError(string contentType)
    {
      // arrange
      var mockFile = CreateMockFile("test.csv", contentType, "name,amount\nTest,100");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = Guid.NewGuid().ToString()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.File);
    }

    [Theory]
    [InlineData("data.csv")]
    [InlineData("transactions.csv")]
    [InlineData("export_2024.csv")]
    [InlineData("very_long_filename_with_many_characters.csv")]
    public void File_WhenValidCsvFilename_ShouldNotHaveValidationError(string filename)
    {
      // arrange
      var mockFile = CreateMockFile(filename, "text/csv", "name,amount\nTest,100");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = Guid.NewGuid().ToString()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void File_WhenEmptyFile_ShouldNotHaveValidationError()
    {
      // arrange
      var mockFile = CreateMockFile("empty.csv", "text/csv", "");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = Guid.NewGuid().ToString()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void File_WhenLargeValidCsvFile_ShouldNotHaveValidationError()
    {
      // arrange
      var largeContent = string.Join("\n", Enumerable.Range(1, 1000).Select(i => $"Transaction{i},{i * 10}"));
      var mockFile = CreateMockFile("large.csv", "text/csv", largeContent);
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = Guid.NewGuid().ToString()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.File);
    }
  }

  public class CorrelationIdValidationTests : UploadCsvDtoValidatorTests
  {
    [Fact]
    public void CorrelationId_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var mockFile = CreateMockFile("test.csv", "text/csv", "name,amount\nTest,100");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = string.Empty
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.CorrelationId)
        .WithErrorMessage("CorrelationId is required.");
    }

    [Fact]
    public void CorrelationId_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var mockFile = CreateMockFile("test.csv", "text/csv", "name,amount\nTest,100");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = null!
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.CorrelationId)
        .WithErrorMessage("CorrelationId is required.");
    }

    [Fact]
    public void CorrelationId_WhenWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var mockFile = CreateMockFile("test.csv", "text/csv", "name,amount\nTest,100");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = "   "
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.CorrelationId)
        .WithErrorMessage("CorrelationId is required.");
    }

    [Theory]
    [InlineData("123e4567-e89b-12d3-a456-426614174000")]
    [InlineData("simple-correlation-id")]
    [InlineData("12345")]
    [InlineData("abc123")]
    [InlineData("correlation_id_with_underscores")]
    public void CorrelationId_WhenValid_ShouldNotHaveValidationError(string correlationId)
    {
      // arrange
      var mockFile = CreateMockFile("test.csv", "text/csv", "name,amount\nTest,100");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = correlationId
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.CorrelationId);
    }

    [Fact]
    public void CorrelationId_WhenGuid_ShouldNotHaveValidationError()
    {
      // arrange
      var mockFile = CreateMockFile("test.csv", "text/csv", "name,amount\nTest,100");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = Guid.NewGuid().ToString()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.CorrelationId);
    }

    [Fact]
    public void CorrelationId_WhenVeryLong_ShouldNotHaveValidationError()
    {
      // arrange
      var longCorrelationId = CreateStringOfLength(500);
      var mockFile = CreateMockFile("test.csv", "text/csv", "name,amount\nTest,100");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = longCorrelationId
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.CorrelationId);
    }
  }

  public class EdgeCaseTests : UploadCsvDtoValidatorTests
  {
    [Fact]
    public void Dto_WhenAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
      // arrange
      var mockFile = CreateMockFile("test.txt", "text/plain", "content");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = string.Empty
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.File);
      result.ShouldHaveValidationErrorFor(x => x.CorrelationId);
    }

    [Fact]
    public void Dto_WhenFileNullAndCorrelationIdEmpty_ShouldHaveMultipleValidationErrors()
    {
      // arrange
      var dto = new UploadCsvFileDto
      {
        File = null!,
        CorrelationId = string.Empty
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.File)
        .WithErrorMessage("File is required.");
      result.ShouldHaveValidationErrorFor(x => x.CorrelationId)
        .WithErrorMessage("CorrelationId is required.");
    }

    [Fact]
    public void File_WhenFileWithCsvExtensionButWrongContentType_ShouldHaveValidationError()
    {
      // arrange
      var mockFile = CreateMockFile("file.csv", "application/octet-stream", "content");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = Guid.NewGuid().ToString()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.File)
        .WithErrorMessage("Invalid file type. Only CSV files are allowed.");
    }

    [Fact]
    public void File_WhenNonCsvFileWithCsvContentType_ShouldNotHaveValidationError()
    {
      // arrange
      var mockFile = CreateMockFile("file.txt", "text/csv", "name,amount\nTest,100");
      var dto = new UploadCsvFileDto
      {
        File = mockFile.Object,
        CorrelationId = Guid.NewGuid().ToString()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.File);
    }
  }

  private static Mock<IFormFile> CreateMockFile(string fileName, string contentType, string content)
  {
    var mock = new Mock<IFormFile>();
    mock.Setup(f => f.FileName).Returns(fileName);
    mock.Setup(f => f.ContentType).Returns(contentType);
    mock.Setup(f => f.Length).Returns(content.Length);

    var stream = new MemoryStream();
    var writer = new StreamWriter(stream);
    writer.Write(content);
    writer.Flush();
    stream.Position = 0;

    mock.Setup(f => f.OpenReadStream()).Returns(stream);
    mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
        .Returns((Stream target, CancellationToken token) => stream.CopyToAsync(target, token));

    return mock;
  }
}
