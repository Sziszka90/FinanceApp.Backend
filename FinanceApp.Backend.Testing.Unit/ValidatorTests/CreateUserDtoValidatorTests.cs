using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Validators;
using FinanceApp.Backend.Domain.Enums;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class CreateUserDtoValidatorTests
{
  private readonly CreateUserDtoValidator _validator;

  public CreateUserDtoValidatorTests()
  {
    _validator = new CreateUserDtoValidator();
  }

  public class UserNameValidationTests : CreateUserDtoValidatorTests
  {
    [Fact]
    public void UserName_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = string.Empty,
        Email = "test@example.com",
        Password = "ValidPass123@",
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.UserName)
        .WithErrorMessage("'User Name' must not be empty.");
    }

    [Fact]
    public void UserName_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = null!,
        Email = "test@example.com",
        Password = "ValidPass123@",
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void UserName_WhenValid_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validusername",
        Email = "test@example.com",
        Password = "ValidPass123@",
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void UserName_WhenWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "   ",
        Email = "test@example.com",
        Password = "ValidPass123@",
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.UserName);
    }
  }

  public class EmailValidationTests : CreateUserDtoValidatorTests
  {
    [Fact]
    public void Email_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = string.Empty,
        Password = "ValidPass123@",
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Email)
        .WithErrorMessage("Email cannot be empty.");
    }

    [Fact]
    public void Email_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = null!,
        Password = "ValidPass123@",
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Email)
        .WithErrorMessage("Email cannot be empty.");
    }

    [Fact]
    public void Email_WhenInvalidFormat_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "invalid-email",
        Password = "ValidPass123@",
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Email)
        .WithErrorMessage("A valid email address is required.");
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user123@domain.co.uk")]
    [InlineData("firstname.lastname@company.org")]
    [InlineData("test+tag@example.net")]
    public void Email_WhenValidFormat_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = email,
        Password = "ValidPass123@",
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_WhenPlainAddress_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "plainaddress",
        Password = "ValidPass123@",
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_WhenMissingAtSymbol_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "userdomain.com",
        Password = "ValidPass123@",
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_WhenOnlyAtSymbol_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "@",
        Password = "ValidPass123@",
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Email);
    }
  }

  public class PasswordValidationTests : CreateUserDtoValidatorTests
  {
    [Fact]
    public void Password_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "test@example.com",
        Password = string.Empty,
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
        .WithErrorMessage("Password cannot be empty.");
    }

    [Fact]
    public void Password_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "test@example.com",
        Password = null!,
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
        .WithErrorMessage("Password cannot be empty.");
    }

    [Fact]
    public void Password_WhenTooShort_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "test@example.com",
        Password = "Short1@", // 7 characters - too short
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
        .WithErrorMessage("Password must be at least 8 characters long.");
    }

    [Fact]
    public void Password_WhenMissingUppercase_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "test@example.com",
        Password = "lowercase123@", // No uppercase
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
        .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Password_WhenMissingLowercase_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "test@example.com",
        Password = "UPPERCASE123@", // No lowercase
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
        .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void Password_WhenMissingDigit_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "test@example.com",
        Password = "Password@", // No digit
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
        .WithErrorMessage("Password must contain at least one digit.");
    }

    [Fact]
    public void Password_WhenMissingSpecialCharacter_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "test@example.com",
        Password = "Password123", // No special character
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
        .WithErrorMessage("Password must contain at least one special character.");
    }

    [Theory]
    [InlineData("ValidPass123@")]
    [InlineData("MySecure123#")]
    [InlineData("StrongPwd456$")]
    [InlineData("Complex789!")]
    public void Password_WhenMeetsAllRequirements_ShouldNotHaveValidationError(string password)
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "test@example.com",
        Password = password,
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
  }

  public class CompleteValidationTests : CreateUserDtoValidatorTests
  {
    [Fact]
    public void Validator_WhenAllFieldsValid_ShouldNotHaveAnyValidationErrors()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validusername",
        Email = "user@example.com",
        Password = "SecurePass123@",
        BaseCurrency = CurrencyEnum.EUR
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_WhenMultipleFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = string.Empty, // Invalid - empty
        Email = "invalid-email", // Invalid - bad format
        Password = "weak", // Invalid - too short, missing requirements
        BaseCurrency = CurrencyEnum.USD
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.UserName);
      result.ShouldHaveValidationErrorFor(x => x.Email);
      result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData(CurrencyEnum.USD)]
    [InlineData(CurrencyEnum.EUR)]
    [InlineData(CurrencyEnum.GBP)]
    public void Validator_WithDifferentBaseCurrencies_WhenOtherFieldsValid_ShouldNotHaveValidationErrors(CurrencyEnum currency)
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "validuser",
        Email = "test@example.com",
        Password = "ValidPass123@",
        BaseCurrency = currency
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }
}
