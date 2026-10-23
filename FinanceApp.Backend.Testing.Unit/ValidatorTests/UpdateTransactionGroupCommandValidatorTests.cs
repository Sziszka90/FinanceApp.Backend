using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.ExpenseTransaction.ExpenseTransactionCommands;
using FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.UpdateTransactionGroup;
using FinanceApp.Backend.Application.Validators;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class UpdateTransactionGroupCommandValidatorTests : ValidatorTestBase
{
  private readonly UpdateTransactionGroupCommandValidator _validator;
  private readonly UpdateTransactionGroupDtoValidator _dtoValidator;

  public UpdateTransactionGroupCommandValidatorTests()
  {
    _dtoValidator = new UpdateTransactionGroupDtoValidator(new MoneyValidator());
    _validator = new UpdateTransactionGroupCommandValidator(_dtoValidator);
  }

  public class ValidCommandTests : UpdateTransactionGroupCommandValidatorTests
  {
    [Fact]
    public void ValidCommand_ShouldNotHaveValidationErrors()
    {
      // arrange
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Group Name",
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class NameValidationTests : UpdateTransactionGroupCommandValidatorTests
  {
    [Fact]
    public void Name_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.NewGuid(),
        Name = string.Empty,
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdateTransactionGroupDto.Name)
        .WithErrorMessage("'Name' must not be empty.");
    }

    [Fact]
    public void Name_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.NewGuid(),
        Name = null!,
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdateTransactionGroupDto.Name)
        .WithErrorMessage("'Name' must not be empty.");
    }

    [Fact]
    public void Name_WhenWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.NewGuid(),
        Name = "   ",
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdateTransactionGroupDto.Name)
        .WithErrorMessage("'Name' must not be empty.");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Transaction Group")]
    [InlineData("Very Long Transaction Group Name That Is Still Valid")]
    public void Name_WhenValid_ShouldNotHaveValidationError(string name)
    {
      // arrange
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.NewGuid(),
        Name = name,
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.UpdateTransactionGroupDto.Name);
    }
  }

  public class DescriptionValidationTests : UpdateTransactionGroupCommandValidatorTests
  {
    [Fact]
    public void Description_WhenNull_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = null,
        GroupIcon = "valid-icon"
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.UpdateTransactionGroupDto.Description);
    }

    [Fact]
    public void Description_WhenEmpty_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = string.Empty,
        GroupIcon = "valid-icon"
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.UpdateTransactionGroupDto.Description);
    }

    [Fact]
    public void Description_WhenExceedsMaxLength_ShouldHaveValidationError()
    {
      // arrange
      var longDescription = CreateStringOfLength(201);
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = longDescription,
        GroupIcon = "valid-icon"
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdateTransactionGroupDto.Description)
        .WithErrorMessage("The length of 'Description' must be 200 characters or fewer. You entered 201 characters.");
    }

    [Theory]
    [InlineData("Short description")]
    [InlineData("A")]
    public void Description_WhenValidLength_ShouldNotHaveValidationError(string description)
    {
      // arrange
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = description,
        GroupIcon = "valid-icon"
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.UpdateTransactionGroupDto.Description);
    }

    [Fact]
    public void Description_WhenExactly200Characters_ShouldNotHaveValidationError()
    {
      // arrange
      var description = CreateStringOfLength(200);
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = description,
        GroupIcon = "valid-icon"
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.UpdateTransactionGroupDto.Description);
    }
  }

  public class GroupIconValidationTests : UpdateTransactionGroupCommandValidatorTests
  {
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("valid-icon")]
    [InlineData("home")]
    [InlineData("shopping-cart")]
    public void GroupIcon_WhenAnyValue_ShouldNotHaveValidationError(string? groupIcon)
    {
      // arrange
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = "Valid description",
        GroupIcon = groupIcon
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.UpdateTransactionGroupDto.GroupIcon);
    }
  }

  public class IdValidationTests : UpdateTransactionGroupCommandValidatorTests
  {
    [Fact]
    public void Id_WhenValidGuid_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.UpdateTransactionGroupDto.Id);
    }

    [Fact]
    public void Id_WhenEmptyGuid_ShouldNotHaveValidationError()
    {
      // Note: The DTO validator doesn't validate the ID as it's handled at the command level
      // arrange
      var dto = new UpdateTransactionGroupDto
      {
        Id = Guid.Empty,
        Name = "Valid Name",
        Description = "Valid description",
        GroupIcon = "valid-icon"
      };
      var command = new UpdateTransactionGroupCommand(Guid.NewGuid(), dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.UpdateTransactionGroupDto.Id);
    }
  }
}
