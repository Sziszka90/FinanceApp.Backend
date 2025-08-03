using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.ExpenseTransaction.ExpenseTransactionCommands;
using FinanceApp.Backend.Application.Validators;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FluentValidation;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class CreateTransactionDtoValidatorTests
{
  private readonly CreateTransactionDtoValidator _validator;
  private readonly MoneyValidator _moneyValidator;

  public CreateTransactionDtoValidatorTests()
  {
    _moneyValidator = new MoneyValidator();
    _validator = new CreateTransactionDtoValidator(_moneyValidator);
  }

  public class NameValidationTests : CreateTransactionDtoValidatorTests
  {
    [Fact]
    public void Name_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = string.Empty,
        Description = "Test description",
        Value = new Money { Amount = 100, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
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
      var dto = new CreateTransactionDto
      {
        Name = null!,
        Description = "Test description",
        Value = new Money { Amount = 100, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_WhenValid_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = "Valid Transaction Name",
        Description = "Test description",
        Value = new Money { Amount = 100, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_WhenWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = "   ",
        Description = "Test description",
        Value = new Money { Amount = 100, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Name);
    }
  }

  public class DescriptionValidationTests : CreateTransactionDtoValidatorTests
  {
    [Fact]
    public void Description_WhenNull_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = "Valid Name",
        Description = null,
        Value = new Money { Amount = 100, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_WhenEmpty_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = "Valid Name",
        Description = string.Empty,
        Value = new Money { Amount = 100, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_WhenExactlyMaxLength_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = "Valid Name",
        Description = new string('a', 200), // Exactly 200 characters
        Value = new Money { Amount = 100, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_WhenExceedsMaxLength_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = "Valid Name",
        Description = new string('a', 201), // 201 characters - exceeds limit
        Value = new Money { Amount = 100, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Description)
        .WithErrorMessage("The length of 'Description' must be 200 characters or fewer. You entered 201 characters.");
    }

    [Fact]
    public void Description_WhenValidLength_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = "Valid Name",
        Description = "This is a valid description within the character limit.",
        Value = new Money { Amount = 100, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
  }

  public class ValueValidationTests : CreateTransactionDtoValidatorTests
  {
    [Fact]
    public void Value_WhenMoneyIsValid_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = "Valid Name",
        Description = "Valid description",
        Value = new Money { Amount = 100, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Value_WhenAmountIsZero_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = "Valid Name",
        Description = "Valid description",
        Value = new Money { Amount = 0, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor("Value.Amount")
        .WithErrorMessage("'Amount' must be greater than '0'.");
    }

    [Fact]
    public void Value_WhenAmountIsNegative_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = "Valid Name",
        Description = "Valid description",
        Value = new Money { Amount = -50, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor("Value.Amount")
        .WithErrorMessage("'Amount' must be greater than '0'.");
    }

    [Fact]
    public void Value_WhenAmountIsPositive_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = "Valid Name",
        Description = "Valid description",
        Value = new Money { Amount = 0.01m, Currency = CurrencyEnum.USD },
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor("Value.Amount");
    }
  }

  public class CompleteValidationTests : CreateTransactionDtoValidatorTests
  {
    [Fact]
    public void Validator_WhenAllFieldsValid_ShouldNotHaveAnyValidationErrors()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = "Valid Transaction Name",
        Description = "Valid description within character limit",
        Value = new Money { Amount = 150.75m, Currency = CurrencyEnum.EUR },
        TransactionDate = DateTimeOffset.UtcNow.AddDays(-1),
        TransactionType = TransactionTypeEnum.Income,
        TransactionGroupId = Guid.NewGuid().ToString()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_WhenMultipleFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
      // arrange
      var dto = new CreateTransactionDto
      {
        Name = string.Empty, // Invalid - empty
        Description = new string('x', 250), // Invalid - too long
        Value = new Money { Amount = -10, Currency = CurrencyEnum.USD }, // Invalid - negative
        TransactionDate = DateTimeOffset.UtcNow,
        TransactionType = TransactionTypeEnum.Expense
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Name);
      result.ShouldHaveValidationErrorFor(x => x.Description);
      result.ShouldHaveValidationErrorFor("Value.Amount");
    }
  }
}
