using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Validators;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class EmailDtoValidatorTests : ValidatorTestBase
{
  private readonly EmailDtoValidator _validator;

  public EmailDtoValidatorTests()
  {
    _validator = new EmailDtoValidator();
  }

  public class ValidEmailTests : EmailDtoValidatorTests
  {
    [Fact]
    public void ValidEmail_ShouldNotHaveValidationErrors()
    {
      // arrange
      var dto = new EmailDto
      {
        Email = CreateValidEmail()
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class EmailValidationTests : EmailDtoValidatorTests
  {
    [Fact]
    public void Email_WhenEmpty_ShouldHaveValidationError()
    {
      // arrange
      var dto = new EmailDto
      {
        Email = string.Empty
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
      var dto = new EmailDto
      {
        Email = null!
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Email)
        .WithErrorMessage("Email cannot be empty.");
    }

    [Fact]
    public void Email_WhenWhitespace_ShouldHaveValidationError()
    {
      // arrange
      var dto = new EmailDto
      {
        Email = "   "
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Email)
        .WithErrorMessage("Email cannot be empty.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Email_WhenInvalidFormat_ShouldHaveValidationError(string email)
    {
      // arrange
      var dto = new EmailDto
      {
        Email = email
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldHaveValidationErrorFor(x => x.Email)
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
      var dto = new EmailDto
      {
        Email = email
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("user@example.COM")]
    [InlineData("USER@example.com")]
    [InlineData("User.Name@Example.Com")]
    public void Email_WhenMixedCase_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var dto = new EmailDto
      {
        Email = email
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("user+filter@example.com")]
    [InlineData("user.name+tag+sorting@example.com")]
    [InlineData("user-name@example.com")]
    [InlineData("user_name@example.com")]
    public void Email_WhenContainsValidSpecialCharacters_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var dto = new EmailDto
      {
        Email = email
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_WhenLongValidEmail_ShouldNotHaveValidationError()
    {
      // arrange
      var longLocalPart = CreateStringOfLength(60, 'a');
      var email = $"{longLocalPart}@example.com";
      var dto = new EmailDto
      {
        Email = email
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("test@example.travel")]
    [InlineData("user@subdomain.example.museum")]
    [InlineData("contact@my-company.business")]
    public void Email_WhenHasLongTopLevelDomain_ShouldNotHaveValidationError(string email)
    {
      // arrange
      var dto = new EmailDto
      {
        Email = email
      };

      // act & assert
      var result = _validator.TestValidate(dto);
      result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
  }
}
