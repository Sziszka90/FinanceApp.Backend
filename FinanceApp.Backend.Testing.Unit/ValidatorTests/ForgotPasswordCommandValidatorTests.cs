using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.UserApi.UserCommands.ForgotPassword;
using FinanceApp.Backend.Application.Validators;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class ForgotPasswordCommandValidatorTests : ValidatorTestBase
{
  private readonly ForgotPasswordCommandValidator _validator;
  private readonly EmailDtoValidator _emailDtoValidator;

  public ForgotPasswordCommandValidatorTests()
  {
    _emailDtoValidator = new EmailDtoValidator();
    _validator = new ForgotPasswordCommandValidator(_emailDtoValidator);
  }

  public class ValidCommandTests : ForgotPasswordCommandValidatorTests
  {
    [Fact]
    public void ValidCommand_ShouldNotHaveValidationErrors()
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = CreateValidEmail()
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class EmailValidationTests : ForgotPasswordCommandValidatorTests
  {
    [Fact]
    public void Email_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = string.Empty
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.EmailDto.Email)
        .WithErrorMessage("Email cannot be empty.");
    }

    [Fact]
    public void Email_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = null!
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.EmailDto.Email)
        .WithErrorMessage("Email cannot be empty.");
    }

    [Fact]
    public void Email_WhenWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = "   "
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.EmailDto.Email)
        .WithErrorMessage("Email cannot be empty.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Email_WhenInvalidFormat_ShouldHaveValidationError(string email)
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = email
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.EmailDto.Email)
        .WithErrorMessage("A valid email address is required.");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email@domain.co.uk")]
    [InlineData("user+tag@example.org")]
    [InlineData("firstname.lastname@company.com")]
    [InlineData("a@b.co")]
    [InlineData("user123@test-domain.com")]
    [InlineData("user_name@subdomain.example.org")]
    public void Email_WhenValidFormat_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = email
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }

    [Fact]
    public void Email_WhenLongValidEmail_ShouldNotHaveValidationError()
    {
      // arrange
      var longLocalPart = CreateStringOfLength(60, 'a');
      var email = $"{longLocalPart}@example.com";
      var emailDto = new EmailDto
      {
        Email = email
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }

    [Theory]
    [InlineData("user@example.COM")]
    [InlineData("USER@example.com")]
    [InlineData("User.Name@Example.Com")]
    public void Email_WhenMixedCase_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = email
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }

    [Theory]
    [InlineData("user+filter@example.com")]
    [InlineData("user.name+tag+sorting@example.com")]
    [InlineData("user-name@example.com")]
    [InlineData("user_name@example.com")]
    public void Email_WhenContainsValidSpecialCharacters_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = email
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }
  }

  public class CancellationTokenValidationTests : ForgotPasswordCommandValidatorTests
  {
    [Fact]
    public void CancellationToken_WhenAnyValue_ShouldNotHaveValidationError()
    {
      // arrange
      using var cts = new CancellationTokenSource();
      var emailDto = new EmailDto
      {
        Email = CreateValidEmail()
      };
      var command = new ForgotPasswordCommand(emailDto, cts.Token);

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
      var emailDto = new EmailDto
      {
        Email = CreateValidEmail()
      };
      var command = new ForgotPasswordCommand(emailDto, cts.Token);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CancellationToken);
    }

    [Fact]
    public void CancellationToken_WhenNone_ShouldNotHaveValidationError()
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = CreateValidEmail()
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.CancellationToken);
    }
  }

  public class EmailDtoValidationTests : ForgotPasswordCommandValidatorTests
  {
    [Fact]
    public void EmailDto_WhenNull_ShouldHaveValidationError()
    {
      // arrange
      var command = new ForgotPasswordCommand(null!, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.EmailDto);
    }

    [Fact]
    public void EmailDto_WhenValid_ShouldNotHaveValidationError()
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = CreateValidEmail()
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto);
    }
  }

  public class EdgeCaseTests : ForgotPasswordCommandValidatorTests
  {
    [Fact]
    public void Command_WhenEmailHasInternationalDomain_ShouldNotHaveValidationError()
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = "user@example.中国"
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      // Note: This test assumes the email validator supports international domains
      // If it doesn't, this test should expect a validation error
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }

    [Fact]
    public void Command_WhenEmailIsVeryLong_ShouldNotHaveValidationError()
    {
      // arrange
      var longEmail = CreateStringOfLength(50, 'a') + "@" + CreateStringOfLength(60, 'b') + ".com";
      var emailDto = new EmailDto
      {
        Email = longEmail
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }

    [Theory]
    [InlineData("test@example.travel")]
    [InlineData("user@subdomain.example.museum")]
    [InlineData("contact@my-company.business")]
    public void Email_WhenHasLongTopLevelDomain_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = email
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }

    [Fact]
    public void Command_WhenEmailContainsNumbers_ShouldNotHaveValidationError()
    {
      // arrange
      var emailDto = new EmailDto
      {
        Email = "user123@domain456.com"
      };
      var command = new ForgotPasswordCommand(emailDto, CancellationToken.None);

      // act & assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.EmailDto.Email);
    }
  }
}
