using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.UserApi.UserCommands.UpdatePassword;
using FinanceApp.Backend.Application.Validators;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class UpdatePasswordCommandValidatorTests : ValidatorTestBase
{
  private readonly UpdatePasswordCommandValidator _validator;
  private readonly UpdatePasswordRequestValidator _requestValidator;

  public UpdatePasswordCommandValidatorTests()
  {
    _requestValidator = new UpdatePasswordRequestValidator();
    _validator = new UpdatePasswordCommandValidator(_requestValidator);
  }

  public class ValidCommandTests : UpdatePasswordCommandValidatorTests
  {
    [Fact]
    public void ValidCommand_ShouldNotHaveValidationErrors()
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_password_reset_token_123",
        Password = CreateValidPassword()
      };
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class TokenValidationTests : UpdatePasswordCommandValidatorTests
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto.Token)
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto.Token)
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto.Token)
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.UpdatePasswordDto.Token);
    }
  }

  public class PasswordValidationTests : UpdatePasswordCommandValidatorTests
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto.Password)
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto.Password)
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto.Password)
        .WithErrorMessage("Password must be at least 8 characters long.");
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.UpdatePasswordDto.Password);
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto.Password)
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto.Password)
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto.Password)
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto.Password)
        .WithErrorMessage("Password must contain at least one special character.");
    }
  }

  public class CancellationTokenValidationTests : UpdatePasswordCommandValidatorTests
  {
    [Fact]
    public void CancellationToken_WhenAnyValue_ShouldNotHaveValidationError()
    {
      // arrange
      using var cts = new CancellationTokenSource();
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_token",
        Password = CreateValidPassword()
      };
      var command = new UpdatePasswordCommand(dto, cts.Token);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CancellationToken);
    }

    [Fact]
    public void CancellationToken_WhenCancelled_ShouldNotHaveValidationError()
    {
      // arrange
      using var cts = new CancellationTokenSource();
      cts.Cancel();
      var dto = new UpdatePasswordRequest
      {
        Token = "valid_token",
        Password = CreateValidPassword()
      };
      var command = new UpdatePasswordCommand(dto, cts.Token);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CancellationToken);
    }
  }

  public class EdgeCaseTests : UpdatePasswordCommandValidatorTests
  {
    [Fact]
    public void Command_WhenAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
      // arrange
      var dto = new UpdatePasswordRequest
      {
        Token = string.Empty,
        Password = "weak"
      };
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto.Token);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto.Password);
    }

    [Fact]
    public void UpdatePasswordDto_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var command = new UpdatePasswordCommand(null!, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.UpdatePasswordDto);
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.UpdatePasswordDto.Token);
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
      var command = new UpdatePasswordCommand(dto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.UpdatePasswordDto.Password);
    }
  }
}
