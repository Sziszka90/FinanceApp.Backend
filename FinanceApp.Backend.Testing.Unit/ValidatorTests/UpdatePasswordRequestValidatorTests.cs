using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Validators;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class UpdatePasswordRequestValidatorTests : ValidatorTestBase
{
  private readonly UpdatePasswordRequestValidator _validator;

  public UpdatePasswordRequestValidatorTests()
  {
    _validator = new UpdatePasswordRequestValidator();
  }

  public class ValidRequestTests : UpdatePasswordRequestValidatorTests
  {
    [Fact]
    public void ValidRequest_ShouldNotHaveValidationErrors()
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_password_reset_token_123",
        Password = CreateValidPassword()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class TokenValidationTests : UpdatePasswordRequestValidatorTests
  {
    [Fact]
    public void Token_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = string.Empty,
        Password = CreateValidPassword()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage("'Token' must not be empty.");
    }

    [Fact]
    public void Token_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = null!,
        Password = CreateValidPassword()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage("'Token' must not be empty.");
    }

    [Fact]
    public void Token_WhenWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = "   ",
        Password = CreateValidPassword()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage("'Token' must not be empty.");
    }

    [Theory]
    [InlineData("valid_token")]
    [InlineData("password_reset_token_123")]
    [InlineData("jwt.token.here")]
    [InlineData("a")]
    [InlineData("very_long_token_with_many_characters_and_numbers_123456789")]
    public void Token_WhenValid_ShouldNotHaveValidationError(string token)
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = token,
        Password = CreateValidPassword()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
  }

  public class PasswordValidationTests : UpdatePasswordRequestValidatorTests
  {
    [Fact]
    public void Password_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_token",
        Password = string.Empty
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
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_token",
        Password = null!
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
        .WithErrorMessage("Password cannot be empty.");
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("short")]
    [InlineData("1234")]
    public void Password_WhenTooShort_ShouldHaveValidationError(string password)
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_token",
        Password = password
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
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
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_token",
        Password = password
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password);
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
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_token",
        Password = password
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_WhenMissingUppercase_ShouldHaveSpecificValidationError()
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_token",
        Password = "password123@"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
        .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Password_WhenMissingLowercase_ShouldHaveSpecificValidationError()
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_token",
        Password = "PASSWORD123@"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
        .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void Password_WhenMissingDigit_ShouldHaveSpecificValidationError()
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_token",
        Password = "Password@"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
        .WithErrorMessage("Password must contain at least one digit.");
    }

    [Fact]
    public void Password_WhenMissingSpecialCharacter_ShouldHaveSpecificValidationError()
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_token",
        Password = "Password123"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Password)
        .WithErrorMessage("Password must contain at least one special character.");
    }
  }

  public class EdgeCaseTests : UpdatePasswordRequestValidatorTests
  {
    [Fact]
    public void Request_WhenAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = string.Empty,
        Password = "weak"
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Token);
      result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Token_WhenContainsSpecialCharacters_ShouldNotHaveValidationError()
    {
      // arrange
      var tokenWithSpecialChars = "token_with-special.chars+and=symbols&more!@#$%^&*()";
      var dto = new UpdatePasswordRequest
      {
        Token = tokenWithSpecialChars,
        Password = CreateValidPassword()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Password_WhenVeryLong_ShouldNotHaveValidationError()
    {
      // arrange
      var longPassword = CreateValidPassword(50);
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_token",
        Password = longPassword
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
  }
}
