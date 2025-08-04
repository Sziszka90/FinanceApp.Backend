using FinanceApp.Backend.Application.AuthApi.AuthCommands.ValidateToken;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class ValidateTokenCommandValidatorTests : ValidatorTestBase
{
  private readonly ValidateTokenCommandValidator _validator;

  public ValidateTokenCommandValidatorTests()
  {
    _validator = new ValidateTokenCommandValidator();
  }

  public class ValidCommandTests : ValidateTokenCommandValidatorTests
  {
    [Fact]
    public void ValidCommand_ShouldNotHaveValidationErrors()
    {
      // arrange
      var command = new ValidateTokenCommand("valid_token_123", CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class TokenValidationTests : ValidateTokenCommandValidatorTests
  {
    [Fact]
    public void Token_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var command = new ValidateTokenCommand(string.Empty, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage("Token is required.");
    }

    [Fact]
    public void Token_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var command = new ValidateTokenCommand(null!, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage("Token is required.");
    }

    [Fact]
    public void Token_WhenWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var command = new ValidateTokenCommand("   ", CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage("Token is required.");
    }

    [Theory]
    [InlineData("valid_token")]
    [InlineData("jwt_token_123")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9")]
    [InlineData("a")]
    [InlineData("token_with_special_chars!@#$%")]
    [InlineData("very_long_token_with_many_characters_and_numbers_123456789")]
    public void Token_WhenValid_ShouldNotHaveValidationError(string token)
    {
      // arrange
      var command = new ValidateTokenCommand(token, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Token_WhenContainsJwtFormat_ShouldNotHaveValidationError()
    {
      // arrange
      var jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
      var command = new ValidateTokenCommand(jwtToken, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Token_WhenVeryLong_ShouldNotHaveValidationError()
    {
      // arrange
      var longToken = CreateStringOfLength(1000);
      var command = new ValidateTokenCommand(longToken, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
  }

  public class CancellationTokenValidationTests : ValidateTokenCommandValidatorTests
  {
    [Fact]
    public void CancellationToken_WhenAnyValue_ShouldNotHaveValidationError()
    {
      // arrange
      using var cts = new CancellationTokenSource();
      var command = new ValidateTokenCommand("valid_token", cts.Token);

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
      var command = new ValidateTokenCommand("valid_token", cts.Token);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CancellationToken);
    }

    [Fact]
    public void CancellationToken_WhenDefault_ShouldNotHaveValidationError()
    {
      // arrange
      var command = new ValidateTokenCommand("valid_token", CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CancellationToken);
    }
  }

  public class EdgeCaseTests : ValidateTokenCommandValidatorTests
  {
    [Theory]
    [InlineData("token-with-dashes")]
    [InlineData("token.with.dots")]
    [InlineData("token_with_underscores")]
    [InlineData("TokenWithMixedCase")]
    [InlineData("token123WithNumbers")]
    [InlineData("token+with=special&chars")]
    public void Token_WhenValidFormat_ShouldNotHaveValidationError(string token)
    {
      // arrange
      var command = new ValidateTokenCommand(token, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Token_WhenTabCharacter_ShouldHaveValidationError()
    {
      // arrange
      var command = new ValidateTokenCommand("\t", CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage("Token is required.");
    }

    [Fact]
    public void Token_WhenNewlineCharacter_ShouldHaveValidationError()
    {
      // arrange
      var command = new ValidateTokenCommand("\n", CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage("Token is required.");
    }

    [Fact]
    public void Token_WhenMixedWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var command = new ValidateTokenCommand(" \t\n ", CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage("Token is required.");
    }

    [Fact]
    public void Token_WhenContainsWhitespaceInMiddle_ShouldNotHaveValidationError()
    {
      // arrange
      var command = new ValidateTokenCommand("token with spaces", CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Token_WhenMinimalLength_ShouldNotHaveValidationError()
    {
      // arrange
      var command = new ValidateTokenCommand("a", CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Token_WhenUnicodeCharacters_ShouldNotHaveValidationError()
    {
      // arrange
      var command = new ValidateTokenCommand("token_with_unicode_🔑", CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
  }
}
