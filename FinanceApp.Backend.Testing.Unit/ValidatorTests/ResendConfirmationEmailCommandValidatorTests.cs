using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.UserApi.UserCommands.ResendConfirmationEmail;
using FinanceApp.Backend.Application.Validators;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class ResendConfirmationEmailCommandValidatorTests : ValidatorTestBase
{
  private readonly ResendConfirmationEmailCommandValidator _validator;

  public ResendConfirmationEmailCommandValidatorTests()
  {
    var emailDtoValidator = new EmailDtoValidator();
    _validator = new ResendConfirmationEmailCommandValidator(emailDtoValidator);
  }

  private static EmailDto CreateEmailDto(string email) => new() { Email = email };

  public class ValidCommandTests : ResendConfirmationEmailCommandValidatorTests
  {
    [Fact]
    public void ValidCommand_ShouldNotHaveValidationErrors()
    {
      // arrange
      var command = new ResendConfirmationEmailCommand(CreateEmailDto(CreateValidEmail()), CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class EmailValidationTests : ResendConfirmationEmailCommandValidatorTests
  {
    [Fact]
    public void Email_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var command = new ResendConfirmationEmailCommand(CreateEmailDto(string.Empty), CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.EmailDto.Email)
        .WithErrorMessage("A valid email address is required.");
    }

    [Fact]
    public void Email_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var command = new ResendConfirmationEmailCommand(CreateEmailDto(null!), CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.EmailDto.Email)
        .WithErrorMessage("Email cannot be empty.");
    }

    [Fact]
    public void Email_WhenWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var command = new ResendConfirmationEmailCommand(CreateEmailDto("   "), CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.EmailDto.Email)
        .WithErrorMessage("A valid email address is required.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user.example.com")]
    [InlineData("user@@example.com")]
    public void Email_WhenInvalidFormat_ShouldHaveValidationError(string email)
    {
      // arrange
      var command = new ResendConfirmationEmailCommand(CreateEmailDto(email), CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.EmailDto.Email)
        .WithErrorMessage("A valid email address is required.");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.org")]
    [InlineData("admin@company.co.uk")]
    [InlineData("user123@test-domain.net")]
    [InlineData("first.last+tag@example.io")]
    [InlineData("a@b.co")]
    public void Email_WhenValidFormat_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var command = new ResendConfirmationEmailCommand(CreateEmailDto(email), CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }
  }

  public class CancellationTokenValidationTests : ResendConfirmationEmailCommandValidatorTests
  {
    [Fact]
    public void CancellationToken_WhenAnyValue_ShouldNotHaveValidationError()
    {
      // arrange
      using var cts = new CancellationTokenSource();
      var command = new ResendConfirmationEmailCommand(CreateEmailDto(CreateValidEmail()), cts.Token);

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
      var command = new ResendConfirmationEmailCommand(CreateEmailDto(CreateValidEmail()), cts.Token);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CancellationToken);
    }

    [Fact]
    public void CancellationToken_WhenDefault_ShouldNotHaveValidationError()
    {
      // arrange
      var command = new ResendConfirmationEmailCommand(CreateEmailDto(CreateValidEmail()), CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CancellationToken);
    }
  }

  public class EdgeCaseTests : ResendConfirmationEmailCommandValidatorTests
  {
    [Theory]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("User@Example.Com")]
    [InlineData("user@EXAMPLE.COM")]
    public void Email_WhenDifferentCasing_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var command = new ResendConfirmationEmailCommand(CreateEmailDto(email), CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }

    [Theory]
    [InlineData("user+tag@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user_name@example.com")]
    [InlineData("user-name@example.com")]
    [InlineData("123user@example.com")]
    [InlineData("user123@example.com")]
    public void Email_WhenValidSpecialCharacters_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var command = new ResendConfirmationEmailCommand(CreateEmailDto(email), CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }

    [Theory]
    [InlineData("user@subdomain.example.com")]
    [InlineData("user@example.co.uk")]
    [InlineData("user@example.museum")]
    [InlineData("user@localhost")]
    public void Email_WhenValidDomainFormats_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var command = new ResendConfirmationEmailCommand(CreateEmailDto(email), CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }

    [Fact]
    public void Email_WhenMinimalLength_ShouldNotHaveValidationError()
    {
      // arrange
      var command = new ResendConfirmationEmailCommand(CreateEmailDto("a@b.co"), CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Email_WhenInvalidDotPlacement_ShouldHaveValidationError(string email)
    {
      // arrange
      var command = new ResendConfirmationEmailCommand(CreateEmailDto(email), CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.EmailDto.Email)
        .WithErrorMessage("A valid email address is required.");
    }
  }
}
