using FinanceApp.Backend.Application.ExpenseTransaction.ExpenseTransactionCommands;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class MoneyValidatorTests
{
  private readonly MoneyValidator _validator;

  public MoneyValidatorTests()
  {
    _validator = new MoneyValidator();
  }

  public class AmountValidationTests : MoneyValidatorTests
  {
    [Fact]
    public void Amount_WhenZero_ShouldHaveValidationError()
    {
      // arrange
      var money = new Money { Amount = 0, Currency = CurrencyEnum.USD };

      // act & assert
      var result = _validator.TestValidate(money);
      result.ShouldHaveValidationErrorFor(x => x.Amount)
        .WithErrorMessage("'Amount' must not be empty.");
    }

    [Fact]
    public void Amount_WhenNegative_ShouldHaveValidationError()
    {
      // arrange
      var money = new Money { Amount = -100, Currency = CurrencyEnum.USD };

      // act & assert
      var result = _validator.TestValidate(money);
      result.ShouldHaveValidationErrorFor(x => x.Amount)
        .WithErrorMessage("'Amount' must be greater than '0'.");
    }

    [Fact]
    public void Amount_WhenPositiveInteger_ShouldNotHaveValidationError()
    {
      // arrange
      var money = new Money { Amount = 100, Currency = CurrencyEnum.USD };

      // act & assert
      var result = _validator.TestValidate(money);
      result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Amount_WhenPositiveDecimal_ShouldNotHaveValidationError()
    {
      // arrange
      var money = new Money { Amount = 99.99m, Currency = CurrencyEnum.EUR };

      // act & assert
      var result = _validator.TestValidate(money);
      result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Amount_WhenVerySmallPositive_ShouldNotHaveValidationError()
    {
      // arrange
      var money = new Money { Amount = 0.01m, Currency = CurrencyEnum.GBP };

      // act & assert
      var result = _validator.TestValidate(money);
      result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Amount_WhenLargeAmount_ShouldNotHaveValidationError()
    {
      // arrange
      var money = new Money { Amount = 999999.99m, Currency = CurrencyEnum.USD };

      // act & assert
      var result = _validator.TestValidate(money);
      result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }
  }

  public class CompleteValidationTests : MoneyValidatorTests
  {
    [Fact]
    public void Money_WhenValid_ShouldNotHaveAnyValidationErrors()
    {
      // arrange
      var money = new Money { Amount = 250.50m, Currency = CurrencyEnum.USD };

      // act & assert
      var result = _validator.TestValidate(money);
      result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(CurrencyEnum.USD)]
    [InlineData(CurrencyEnum.EUR)]
    [InlineData(CurrencyEnum.GBP)]
    public void Money_WithDifferentCurrencies_WhenAmountValid_ShouldNotHaveValidationErrors(CurrencyEnum currency)
    {
      // arrange
      var money = new Money { Amount = 100, Currency = currency };

      // act & assert
      var result = _validator.TestValidate(money);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }
}
