using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.UserApi.UserCommands.CreateUser;
using FinanceApp.Backend.Application.Validators;
using FinanceApp.Backend.Domain.Enums;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class CreateUserCommandValidatorTests : ValidatorTestBase
{
  private readonly CreateUserCommandValidator _validator;
  private readonly CreateUserDtoValidator _dtoValidator;

  public CreateUserCommandValidatorTests()
  {
    _dtoValidator = new CreateUserDtoValidator();
    _validator = new CreateUserCommandValidator(_dtoValidator);
  }

  public class ValidCommandTests : CreateUserCommandValidatorTests
  {
    [Fact]
    public void ValidCommand_ShouldNotHaveValidationErrors()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = CreateValidPassword(),
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class UserNameValidationTests : CreateUserCommandValidatorTests
  {
    [Fact]
    public void UserName_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = string.Empty,
        Email = CreateValidEmail(),
        Password = CreateValidPassword(),
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.UserName)
        .WithErrorMessage("'User Name' must not be empty.");
    }

    [Fact]
    public void UserName_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = null!,
        Email = CreateValidEmail(),
        Password = CreateValidPassword(),
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.UserName)
        .WithErrorMessage("'User Name' must not be empty.");
    }

    [Fact]
    public void UserName_WhenWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "   ",
        Email = CreateValidEmail(),
        Password = CreateValidPassword(),
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.UserName)
        .WithErrorMessage("'User Name' must not be empty.");
    }

    [Theory]
    [InlineData("user")]
    [InlineData("testuser123")]
    [InlineData("user_name")]
    [InlineData("user-name")]
    [InlineData("user.name")]
    [InlineData("a")]
    public void UserName_WhenValid_ShouldNotHaveValidationError(string userName)
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = userName,
        Email = CreateValidEmail(),
        Password = CreateValidPassword(),
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CreateUserDto.UserName);
    }
  }

  public class EmailValidationTests : CreateUserCommandValidatorTests
  {
    [Fact]
    public void Email_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = string.Empty,
        Password = CreateValidPassword(),
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Email)
        .WithErrorMessage("Email cannot be empty.");
    }

    [Fact]
    public void Email_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = null!,
        Password = CreateValidPassword(),
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Email)
        .WithErrorMessage("Email cannot be empty.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Email_WhenInvalidFormat_ShouldHaveValidationError(string email)
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = email,
        Password = CreateValidPassword(),
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Email)
        .WithErrorMessage("A valid email address is required.");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email@domain.co.uk")]
    [InlineData("user+tag@example.org")]
    [InlineData("firstname.lastname@company.com")]
    [InlineData("a@b.co")]
    public void Email_WhenValidFormat_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = email,
        Password = CreateValidPassword(),
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CreateUserDto.Email);
    }
  }

  public class PasswordValidationTests : CreateUserCommandValidatorTests
  {
    [Fact]
    public void Password_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = string.Empty,
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Password)
        .WithErrorMessage("Password cannot be empty.");
    }

    [Fact]
    public void Password_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = null!,
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Password)
        .WithErrorMessage("Password cannot be empty.");
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("short")]
    [InlineData("1234")]
    public void Password_WhenTooShort_ShouldHaveValidationError(string password)
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = password,
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Password)
        .WithErrorMessage("Password must be at least 8 characters long.");
    }

    [Theory]
    [InlineData("password123@")]
    [InlineData("PASSWORD123@")]
    [InlineData("Password123")]
    [InlineData("Password@")]
    [InlineData("password@")]
    public void Password_WhenMissingRequirements_ShouldHaveValidationError(string password)
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = password,
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Password);
    }

    [Theory]
    [InlineData("Password123@")]
    [InlineData("MySecure123#")]
    [InlineData("StrongPwd456$")]
    [InlineData("Complex789!")]
    [InlineData("ValidPass123%")]
    public void Password_WhenMeetsAllRequirements_ShouldNotHaveValidationError(string password)
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = password,
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CreateUserDto.Password);
    }

    [Fact]
    public void Password_WhenMissingUppercase_ShouldHaveSpecificValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = "password123@",
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Password)
        .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Password_WhenMissingLowercase_ShouldHaveSpecificValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = "PASSWORD123@",
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Password)
        .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void Password_WhenMissingDigit_ShouldHaveSpecificValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = "Password@",
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Password)
        .WithErrorMessage("Password must contain at least one digit.");
    }

    [Fact]
    public void Password_WhenMissingSpecialCharacter_ShouldHaveSpecificValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = "Password123",
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Password)
        .WithErrorMessage("Password must contain at least one special character.");
    }
  }

  public class BaseCurrencyValidationTests : CreateUserCommandValidatorTests
  {
    [Theory]
    [InlineData(CurrencyEnum.USD)]
    [InlineData(CurrencyEnum.EUR)]
    [InlineData(CurrencyEnum.GBP)]
    [InlineData(CurrencyEnum.HUF)]
    public void BaseCurrency_WhenValidEnum_ShouldNotHaveValidationError(CurrencyEnum currency)
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = CreateValidPassword(),
        BaseCurrency = currency
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CreateUserDto.BaseCurrency);
    }

    [Fact]
    public void BaseCurrency_WhenInvalidEnum_ShouldHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = CreateValidPassword(),
        BaseCurrency = (CurrencyEnum)999 // Invalid enum value
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.BaseCurrency);
    }
  }

  public class CancellationTokenValidationTests : CreateUserCommandValidatorTests
  {
    [Fact]
    public void CancellationToken_WhenAnyValue_ShouldNotHaveValidationError()
    {
      // arrange
      using var cts = new CancellationTokenSource();
      var dto = new CreateUserDto
      {
        UserName = "testuser",
        Email = CreateValidEmail(),
        Password = CreateValidPassword(),
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, cts.Token);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CancellationToken);
    }
  }

  public class EdgeCaseTests : CreateUserCommandValidatorTests
  {
    [Fact]
    public void Command_WhenAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = string.Empty,
        Email = "invalid-email",
        Password = "weak",
        BaseCurrency = (CurrencyEnum)999
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.UserName);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Email);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.Password);
      result.ShouldHaveValidationErrorFor(x => x.CreateUserDto.BaseCurrency);
    }

    [Fact]
    public void Command_WhenUserNameWithUnicodeCharacters_ShouldNotHaveValidationError()
    {
      // arrange
      var dto = new CreateUserDto
      {
        UserName = "üsêr_näme_ñõñ_äśçíí",
        Email = CreateValidEmail(),
        Password = CreateValidPassword(),
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CreateUserDto.UserName);
    }

    [Fact]
    public void Command_WhenVeryLongUserName_ShouldNotHaveValidationError()
    {
      // arrange
      var longUserName = CreateStringOfLength(200);
      var dto = new CreateUserDto
      {
        UserName = longUserName,
        Email = CreateValidEmail(),
        Password = CreateValidPassword(),
        BaseCurrency = CurrencyEnum.USD
      };
      var command = new CreateUserCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CreateUserDto.UserName);
    }
  }
}
