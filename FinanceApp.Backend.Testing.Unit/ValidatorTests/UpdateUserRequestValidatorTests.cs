using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Validators;
using FinanceApp.Backend.Domain.Enums;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class UpdateUserRequestValidatorTests : ValidatorTestBase
{
  private readonly UpdateUserRequestValidator _validator;

  public UpdateUserRequestValidatorTests()
  {
    _validator = new UpdateUserRequestValidator();
  }

  public class ValidRequestTests : UpdateUserRequestValidatorTests
  {
    [Fact]
    public void ValidRequest_ShouldNotHaveValidationErrors()
    {
      // arrange
      var dto = new UpdateUserRequest
      {
        Id = Guid.NewGuid(),
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class BaseCurrencyValidationTests : UpdateUserRequestValidatorTests
  {
    [Theory]
    [InlineData(CurrencyEnum.USD)]
    [InlineData(CurrencyEnum.EUR)]
    [InlineData(CurrencyEnum.GBP)]
    [InlineData(CurrencyEnum.HUF)]
    public void BaseCurrency_WhenValidEnum_ShouldNotHaveValidationError(CurrencyEnum currency)
    {
      // arrange
      var dto = new UpdateUserRequest
      {
        Id = Guid.NewGuid(),
        BaseCurrency = currency
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.BaseCurrency);
    }

    [Fact]
    public void BaseCurrency_WhenInvalidEnum_ShouldHaveValidationError()
    {
      // arrange
      var dto = new UpdateUserRequest
      {
        Id = Guid.NewGuid(),
        BaseCurrency = (CurrencyEnum)999 // Invalid enum value
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.BaseCurrency)
        .WithErrorMessage("'Base Currency' has a range of values which does not include '999'.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    [InlineData(999)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void BaseCurrency_WhenOutOfRangeValues_ShouldHaveValidationError(int invalidValue)
    {
      // arrange
      var dto = new UpdateUserRequest
      {
        Id = Guid.NewGuid(),
        BaseCurrency = (CurrencyEnum)invalidValue
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.BaseCurrency);
    }
  }

  public class IdValidationTests : UpdateUserRequestValidatorTests
  {
    [Fact]
    public void Id_WhenValidGuid_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new UpdateUserRequest
      {
        Id = Guid.NewGuid(),
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Id_WhenEmptyGuid_ShouldNotHaveValidationError()
    {
      // Note: The validator doesn't validate the ID field directly
      // arrange
      var dto = new UpdateUserRequest
      {
        Id = Guid.Empty,
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("6ba7b810-9dad-11d1-80b4-00c04fd430c8")]
    [InlineData("6ba7b811-9dad-11d1-80b4-00c04fd430c8")]
    public void Id_WhenValidGuidStrings_ShouldNotHaveValidationError(string guidString)
    {
      // arrange
      var validGuid = Guid.Parse(guidString);
      var dto = new UpdateUserRequest
      {
        Id = validGuid,
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
  }

  public class EdgeCaseTests : UpdateUserRequestValidatorTests
  {
    [Fact]
    public void Request_WhenOnlyBaseCurrencyProvided_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new UpdateUserRequest
      {
        Id = Guid.Empty, // Empty ID should be fine since it's not validated
        BaseCurrency = CurrencyEnum.EUR
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Request_WhenAllFieldsValid_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new UpdateUserRequest
      {
        Id = Guid.NewGuid(),
        BaseCurrency = CurrencyEnum.GBP
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(CurrencyEnum.USD, "6ba7b810-9dad-11d1-80b4-00c04fd430c8")]
    [InlineData(CurrencyEnum.EUR, "550e8400-e29b-41d4-a716-446655440000")]
    [InlineData(CurrencyEnum.HUF, "6ba7b811-9dad-11d1-80b4-00c04fd430c8")]
    public void Request_WhenValidCombinations_ShouldNotHaveValidationError(CurrencyEnum currency, string guidString)
    {
      // arrange
      var dto = new UpdateUserRequest
      {
        Id = Guid.Parse(guidString),
        BaseCurrency = currency
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Request_WhenBaseCurrencyIsFirstEnumValue_ShouldNotHaveValidationError()
    {
      // arrange
      var firstEnumValue = Enum.GetValues<CurrencyEnum>().First();
      var dto = new UpdateUserRequest
      {
        Id = Guid.NewGuid(),
        BaseCurrency = firstEnumValue
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.BaseCurrency);
    }

    [Fact]
    public void Request_WhenBaseCurrencyIsLastEnumValue_ShouldNotHaveValidationError()
    {
      // arrange
      var lastEnumValue = Enum.GetValues<CurrencyEnum>().Last();
      var dto = new UpdateUserRequest
      {
        Id = Guid.NewGuid(),
        BaseCurrency = lastEnumValue
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.BaseCurrency);
    }
  }

  public class AllValidCurrenciesTests : UpdateUserRequestValidatorTests
  {
    [Fact]
    public void BaseCurrency_WhenAllValidCurrencies_ShouldNotHaveValidationErrors()
    {
      // This test ensures all defined currency enums are considered valid
      var allCurrencies = Enum.GetValues<CurrencyEnum>();

      foreach (var currency in allCurrencies)
      {
        // arrange
        var dto = new UpdateUserRequest
        {
          Id = Guid.NewGuid(),
          BaseCurrency = currency
        };

        // act & assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.BaseCurrency);
      }
    }
  }
}
