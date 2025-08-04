using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Validators;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class CreateTransactionGroupDtoValidatorTests : ValidatorTestBase
{
  private readonly CreateTransactionGroupDtoValidator _validator;

  public CreateTransactionGroupDtoValidatorTests()
  {
    _validator = new CreateTransactionGroupDtoValidator();
  }

  public class ValidDtoTests : CreateTransactionGroupDtoValidatorTests
  {
    [Fact]
    public void ValidDto_ShouldNotHaveValidationErrors()
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = "Valid Group Name",
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class NameValidationTests : CreateTransactionGroupDtoValidatorTests
  {
    [Fact]
    public void Name_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = string.Empty,
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Name)
        .WithErrorMessage("'Name' must not be empty.");
    }

    [Fact]
    public void Name_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = null!,
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Name)
        .WithErrorMessage("'Name' must not be empty.");
    }

    [Fact]
    public void Name_WhenWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = "   ",
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Name)
        .WithErrorMessage("'Name' must not be empty.");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Food")]
    [InlineData("Transportation")]
    [InlineData("Entertainment & Leisure")]
    [InlineData("Business-Related Expenses")]
    [InlineData("Very Long Transaction Group Name That Is Still Valid")]
    public void Name_WhenValid_ShouldNotHaveValidationError(string name)
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = name,
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_WhenContainsSpecialCharacters_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = "Group Name!@#$%^&*()",
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_WhenContainsUnicodeCharacters_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = "Grüppenäme ñõñ-äśçíí",
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
  }

  public class DescriptionValidationTests : CreateTransactionGroupDtoValidatorTests
  {
    [Fact]
    public void Description_WhenNull_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = "Valid Name",
        Description = null,
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_WhenEmpty_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = "Valid Name",
        Description = string.Empty,
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_WhenExceedsMaxLength_ShouldHaveValidationError()
    {
      // arrange
      var longDescription = CreateStringOfLength(201);
      var dto = new CreateTransactionGroupDto
      {
        Name = "Valid Name",
        Description = longDescription,
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Description)
        .WithErrorMessage("The length of 'Description' must be 200 characters or fewer. You entered 201 characters.");
    }

    [Theory]
    [InlineData("Short description")]
    [InlineData("A")]
    [InlineData("")]
    public void Description_WhenValidLength_ShouldNotHaveValidationError(string description)
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = "Valid Name",
        Description = description,
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_WhenExactly200Characters_ShouldNotHaveValidationError()
    {
      // arrange
      var description = CreateStringOfLength(200);
      var dto = new CreateTransactionGroupDto
      {
        Name = "Valid Name",
        Description = description,
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_WhenContainsSpecialCharacters_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = "Valid Name",
        Description = "Description with special chars!@#$%^&*()",
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_WhenContainsNewlines_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = "Valid Name",
        Description = "Line 1\nLine 2\nLine 3",
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
  }

  public class GroupIconValidationTests : CreateTransactionGroupDtoValidatorTests
  {
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("home")]
    [InlineData("shopping-cart")]
    [InlineData("fas fa-car")]
    [InlineData("🏠")]
    [InlineData("very-long-icon-name-that-should-still-be-valid")]
    public void GroupIcon_WhenAnyValue_ShouldNotHaveValidationError(string? groupIcon)
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = "Valid Name",
        Description = "Valid description",
        GroupIcon = groupIcon
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.GroupIcon);
    }

    [Fact]
    public void GroupIcon_WhenVeryLong_ShouldNotHaveValidationError()
    {
      // arrange
      var longIcon = CreateStringOfLength(500);
      var dto = new CreateTransactionGroupDto
      {
        Name = "Valid Name",
        Description = "Valid description",
        GroupIcon = longIcon
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.GroupIcon);
    }
  }

  public class EdgeCaseTests : CreateTransactionGroupDtoValidatorTests
  {
    [Fact]
    public void Dto_WhenNameInvalidAndDescriptionTooLong_ShouldHaveMultipleValidationErrors()
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = string.Empty,
        Description = CreateStringOfLength(201),
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Name);
      result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Dto_WhenOnlyNameProvided_ShouldNotHaveValidationErrors()
    {
      // arrange
      var dto = new CreateTransactionGroupDto
      {
        Name = "Valid Name",
        Description = null,
        GroupIcon = null
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Name_WhenVeryLong_ShouldNotHaveValidationError()
    {
      // arrange
      var longName = CreateStringOfLength(500);
      var dto = new CreateTransactionGroupDto
      {
        Name = longName,
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
  }
}
