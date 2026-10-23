using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.ExpenseTransaction.ExpenseTransactionCommands;
using FinanceApp.Backend.Application.Validators;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class UpdateTransactionDtoValidatorTests : ValidatorTestBase
{
  private readonly UpdateTransactionDtoValidator _validator;
  private readonly MoneyValidator _moneyValidator;

  public UpdateTransactionDtoValidatorTests()
  {
    _moneyValidator = new MoneyValidator();
    _validator = new UpdateTransactionDtoValidator(_moneyValidator);
  }

  public class ValidDtoTests : UpdateTransactionDtoValidatorTests
  {
    [Fact]
    public void ValidDto_ShouldNotHaveValidationErrors()
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Transaction Name",
        Description = "Valid description",
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class NameValidationTests : UpdateTransactionDtoValidatorTests
  {
    [Fact]
    public void Name_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = string.Empty,
        Description = "Valid description",
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
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
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = null!,
        Description = "Valid description",
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
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
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "   ",
        Description = "Valid description",
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Name)
        .WithErrorMessage("'Name' must not be empty.");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Coffee")]
    [InlineData("Grocery Shopping")]
    [InlineData("Gas Station Payment")]
    [InlineData("Amazon Purchase - Electronics")]
    [InlineData("Very Long Transaction Name That Should Still Be Valid")]
    public void Name_WhenValid_ShouldNotHaveValidationError(string name)
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = name,
        Description = "Valid description",
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_WhenContainsSpecialCharacters_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Transaction Name!@#$%^&*()",
        Description = "Valid description",
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_WhenContainsUnicodeCharacters_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Tränśäçtíön Ñäme ñõñ-äśçíí",
        Description = "Valid description",
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
  }

  public class DescriptionValidationTests : UpdateTransactionDtoValidatorTests
  {
    [Fact]
    public void Description_WhenNull_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = null,
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_WhenEmpty_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = string.Empty,
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
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
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = longDescription,
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
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
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = description,
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
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
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = description,
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
  }

  public class ValueValidationTests : UpdateTransactionDtoValidatorTests
  {
    [Theory]
    [InlineData(0)]
    [InlineData(0.01)]
    [InlineData(100.50)]
    [InlineData(999999.99)]
    public void Value_WhenValidMoney_ShouldNotHaveValidationError(decimal amount)
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = "Valid description",
        Value = new Money { Amount = amount, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Value);
    }

    [Theory]
    [InlineData(CurrencyEnum.USD)]
    [InlineData(CurrencyEnum.EUR)]
    [InlineData(CurrencyEnum.GBP)]
    [InlineData(CurrencyEnum.HUF)]
    public void Value_WhenValidCurrency_ShouldNotHaveValidationError(CurrencyEnum currency)
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = "Valid description",
        Value = new Money { Amount = 100.50m, Currency = currency },
        TransactionGroupId = Guid.NewGuid()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Value_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = "Valid description",
        Value = null!,
        TransactionGroupId = Guid.NewGuid()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Value_WhenNegativeAmount_ShouldHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = "Valid description",
        Value = new Money { Amount = -100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor("Value.Amount")
        .WithErrorMessage("'Amount' must be greater than '0'.");
    }
  }

  public class EdgeCaseTests : UpdateTransactionDtoValidatorTests
  {
    [Fact]
    public void Dto_WhenAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = string.Empty,
        Description = CreateStringOfLength(201),
        Value = new Money { Amount = -100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.NewGuid()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Name);
      result.ShouldHaveValidationErrorFor(x => x.Description);
      result.ShouldHaveValidationErrorFor("Value.Amount");
    }

    [Fact]
    public void Dto_WhenOnlyNameProvided_ShouldHaveValidationErrorForValue()
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = null,
        Value = null!,
        TransactionGroupId = null
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Name);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
      result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void TransactionGroupId_WhenNull_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = "Valid description",
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = null
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.TransactionGroupId);
    }

    [Fact]
    public void TransactionGroupId_WhenEmptyGuid_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new UpdateTransactionDto
      {
        Id = Guid.NewGuid(),
        Name = "Valid Name",
        Description = "Valid description",
        Value = new Money { Amount = 100.50m, Currency = CurrencyEnum.USD },
        TransactionGroupId = Guid.Empty
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.TransactionGroupId);
    }
  }
}
