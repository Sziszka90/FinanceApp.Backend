using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.UserApi.UserCommands.ConfirmUserEmail;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class ConfirmUserEmailCommandValidatorTests : ValidatorTestBase
{
  private readonly ConfirmUserEmailCommandValidator _validator;

  public ConfirmUserEmailCommandValidatorTests()
  {
    _validator = new ConfirmUserEmailCommandValidator();
  }

  public class ValidCommandTests : ConfirmUserEmailCommandValidatorTests
  {
    [Fact]
    public void ValidCommand_ShouldNotHaveValidationErrors()
    {
      // arrange
      var command = new ConfirmUserEmailCommand(
        Id: Guid.NewGuid(),
        Token: "valid_email_confirmation_token_123",
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class TokenValidationTests : ConfirmUserEmailCommandValidatorTests
  {
    [Fact]
    public void Token_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var command = new ConfirmUserEmailCommand(
        Id: Guid.NewGuid(),
        Token: string.Empty,
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage(ApplicationError.TOKEN_NOT_PROVIDED_MESSAGE);
    }

    [Fact]
    public void Token_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var command = new ConfirmUserEmailCommand(
        Id: Guid.NewGuid(),
        Token: null!,
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage(ApplicationError.TOKEN_NOT_PROVIDED_MESSAGE);
    }

    [Fact]
    public void Token_WhenWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var command = new ConfirmUserEmailCommand(
        Id: Guid.NewGuid(),
        Token: "   ",
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage(ApplicationError.TOKEN_NOT_PROVIDED_MESSAGE);
    }

    [Theory]
    [InlineData("valid_token")]
    [InlineData("abc123")]
    [InlineData("email_confirmation_token_12345")]
    [InlineData("jwt.token.here")]
    [InlineData("a")]
    public void Token_WhenValid_ShouldNotHaveValidationError(string token)
    {
      // arrange
      var command = new ConfirmUserEmailCommand(
        Id: Guid.NewGuid(),
        Token: token,
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Token_WhenLongToken_ShouldNotHaveValidationError()
    {
      // arrange
      var longToken = CreateStringOfLength(500, 'x');
      var command = new ConfirmUserEmailCommand(
        Id: Guid.NewGuid(),
        Token: longToken,
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
  }

  public class IdValidationTests : ConfirmUserEmailCommandValidatorTests
  {
    [Fact]
    public void Id_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var command = new ConfirmUserEmailCommand(
        Id: Guid.Empty,
        Token: "valid_token",
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.Id)
        .WithErrorMessage(ApplicationError.USER_ID_NOT_PROVIDED_MESSAGE);
    }

    [Fact]
    public void Id_WhenValidGuid_ShouldNotHaveValidationError()
    {
      // arrange
      var command = new ConfirmUserEmailCommand(
        Id: Guid.NewGuid(),
        Token: "valid_token",
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
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
      var command = new ConfirmUserEmailCommand(
        Id: validGuid,
        Token: "valid_token",
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
  }

  public class CancellationTokenValidationTests : ConfirmUserEmailCommandValidatorTests
  {
    [Fact]
    public void CancellationToken_WhenAnyValue_ShouldNotHaveValidationError()
    {
      // arrange
      using var cts = new CancellationTokenSource();
      var command = new ConfirmUserEmailCommand(
        Id: Guid.NewGuid(),
        Token: "valid_token",
        CancellationToken: cts.Token
      );

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
      var command = new ConfirmUserEmailCommand(
        Id: Guid.NewGuid(),
        Token: "valid_token",
        CancellationToken: cts.Token
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CancellationToken);
    }

    [Fact]
    public void CancellationToken_WhenNone_ShouldNotHaveValidationError()
    {
      // arrange
      var command = new ConfirmUserEmailCommand(
        Id: Guid.NewGuid(),
        Token: "valid_token",
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CancellationToken);
    }
  }

  public class EdgeCaseTests : ConfirmUserEmailCommandValidatorTests
  {
    [Fact]
    public void Command_WhenAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
      // arrange
      var command = new ConfirmUserEmailCommand(
        Id: Guid.Empty,
        Token: string.Empty,
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.Id)
        .WithErrorMessage(ApplicationError.USER_ID_NOT_PROVIDED_MESSAGE);
      result.ShouldHaveValidationErrorFor(x => x.Token)
        .WithErrorMessage(ApplicationError.TOKEN_NOT_PROVIDED_MESSAGE);
    }

    [Fact]
    public void Command_WhenTokenContainsSpecialCharacters_ShouldNotHaveValidationError()
    {
      // arrange
      var tokenWithSpecialChars = "token_with-special.chars+and=symbols&more!@#$%^&*()";
      var command = new ConfirmUserEmailCommand(
        Id: Guid.NewGuid(),
        Token: tokenWithSpecialChars,
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Command_WhenTokenContainsUnicodeCharacters_ShouldNotHaveValidationError()
    {
      // arrange
      var unicodeToken = "tökên_wíth_ûnícödé_¢håråçtérś";
      var command = new ConfirmUserEmailCommand(
        Id: Guid.NewGuid(),
        Token: unicodeToken,
        CancellationToken: CancellationToken.None
      );

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
  }
}
