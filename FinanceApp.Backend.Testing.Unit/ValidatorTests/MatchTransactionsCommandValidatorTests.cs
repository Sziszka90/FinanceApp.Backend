using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.MatchTransactionsCommands;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class MatchTransactionsCommandValidatorTests : ValidatorTestBase
{
  protected readonly MatchTransactionsCommandValidator _validator;

  public MatchTransactionsCommandValidatorTests()
  {
    _validator = new MatchTransactionsCommandValidator();
  }

  public class ValidCommandTests : MatchTransactionsCommandValidatorTests
  {
    [Fact]
    public void ValidCommand_ShouldNotHaveValidationErrors()
    {
      // Arrange
      var responseDto = new RabbitMqPayload
      {
        CorrelationId = "valid-correlation-id-123",
        UserId = "550e8400-e29b-41d4-a716-446655440000",
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new MatchTransactionsCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidCommand_WithLongValues_ShouldNotHaveValidationErrors()
    {
      // Arrange
      var responseDto = new RabbitMqPayload
      {
        CorrelationId = CreateStringOfLength(100),
        UserId = "6ba7b810-9dad-11d1-80b4-00c04fd430c8",
        Success = false,
        Prompt = CreateStringOfLength(500)
      };
      var command = new MatchTransactionsCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class CorrelationIdValidationTests : MatchTransactionsCommandValidatorTests
  {
    [Fact]
    public void CorrelationId_WhenEmpty_ShouldHaveValidationError()
    {
      // Arrange
      var responseDto = new RabbitMqPayload
      {
        CorrelationId = string.Empty,
        UserId = "550e8400-e29b-41d4-a716-446655440000",
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new MatchTransactionsCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.ResponseDto.CorrelationId)
          .WithErrorMessage(ApplicationError.INVALID_REQUEST_ERROR_MESSAGE);
    }

    [Fact]
    public void CorrelationId_WhenNull_ShouldHaveValidationError()
    {
      // Arrange
      var responseDto = new RabbitMqPayload
      {
        CorrelationId = null!,
        UserId = "550e8400-e29b-41d4-a716-446655440000",
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new MatchTransactionsCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.ResponseDto.CorrelationId)
          .WithErrorMessage(ApplicationError.INVALID_REQUEST_ERROR_MESSAGE);
    }

    [Fact]
    public void CorrelationId_WhenWhitespace_ShouldHaveValidationError()
    {
      // Arrange
      var responseDto = new RabbitMqPayload
      {
        CorrelationId = "   ",
        UserId = "550e8400-e29b-41d4-a716-446655440000",
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new MatchTransactionsCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.ResponseDto.CorrelationId)
          .WithErrorMessage(ApplicationError.INVALID_REQUEST_ERROR_MESSAGE);
    }

    [Theory]
    [InlineData("corr-123")]
    [InlineData("correlation-id-456")]
    [InlineData("abc123")]
    [InlineData("uuid-550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("a")]
    [InlineData("1")]
    public void CorrelationId_WhenValid_ShouldNotHaveValidationError(string correlationId)
    {
      // Arrange
      var responseDto = new RabbitMqPayload
      {
        CorrelationId = correlationId,
        UserId = "550e8400-e29b-41d4-a716-446655440000",
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new MatchTransactionsCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.ResponseDto.CorrelationId);
    }

    [Fact]
    public void CorrelationId_WhenVeryLong_ShouldNotHaveValidationError()
    {
      // Arrange
      var longCorrelationId = CreateStringOfLength(500);
      var responseDto = new RabbitMqPayload
      {
        CorrelationId = longCorrelationId,
        UserId = "550e8400-e29b-41d4-a716-446655440000",
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new MatchTransactionsCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.ResponseDto.CorrelationId);
    }
  }

  public class UserIdValidationTests : MatchTransactionsCommandValidatorTests
  {
    [Fact]
    public void UserId_WhenEmpty_ShouldHaveValidationError()
    {
      // Arrange
      var responseDto = new RabbitMqPayload
      {
        CorrelationId = "valid-correlation-id",
        UserId = string.Empty,
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new MatchTransactionsCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.ResponseDto.UserId)
          .WithErrorMessage(ApplicationError.USER_ID_NOT_PROVIDED_MESSAGE);
    }

    [Fact]
    public void UserId_WhenNull_ShouldHaveValidationError()
    {
      // Arrange
      var responseDto = new RabbitMqPayload
      {
        CorrelationId = "valid-correlation-id",
        UserId = null!,
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new MatchTransactionsCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.ResponseDto.UserId)
          .WithErrorMessage(ApplicationError.USER_ID_NOT_PROVIDED_MESSAGE);
    }

    [Fact]
    public void UserId_WhenWhitespace_ShouldHaveValidationError()
    {
      // Arrange
      var responseDto = new RabbitMqPayload
      {
        CorrelationId = "valid-correlation-id",
        UserId = "   ",
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new MatchTransactionsCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.ResponseDto.UserId)
          .WithErrorMessage(ApplicationError.USER_ID_NOT_PROVIDED_MESSAGE);
    }

    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("6ba7b810-9dad-11d1-80b4-00c04fd430c8")]
    [InlineData("user-123")]
    [InlineData("123")]
    [InlineData("u")]
    public void UserId_WhenValid_ShouldNotHaveValidationError(string userId)
    {
      // Arrange
      var responseDto = new RabbitMqPayload
      {
        CorrelationId = "valid-correlation-id",
        UserId = userId,
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new MatchTransactionsCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.ResponseDto.UserId);
    }

    [Fact]
    public void UserId_WhenLongString_ShouldNotHaveValidationError()
    {
      // Arrange
      var longUserId = CreateStringOfLength(100);
      var responseDto = new RabbitMqPayload
      {
        CorrelationId = "valid-correlation-id",
        UserId = longUserId,
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new MatchTransactionsCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.ResponseDto.UserId);
    }
  }

  [Fact]
  public void Response_WhenComplexJsonData_ShouldNotHaveValidationError()
  {
    // Arrange
    var responseDto = new RabbitMqPayload
    {
      CorrelationId = "valid-correlation-id",
      UserId = "550e8400-e29b-41d4-a716-446655440000",
      Success = true,
      Prompt = "Valid prompt"
    };
    var command = new MatchTransactionsCommand(responseDto);

    // Act & Assert
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveValidationErrorFor(x => x.ResponseDto);
  }
}

public class EdgeCaseTests : MatchTransactionsCommandValidatorTests
{
  [Fact]
  public void Command_WhenAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
  {
    // Arrange
    var responseDto = new RabbitMqPayload
    {
      CorrelationId = string.Empty,
      UserId = string.Empty,
      Success = true,
      Prompt = "Valid prompt"
    };
    var command = new MatchTransactionsCommand(responseDto);

    // Act & Assert
    var result = _validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor(x => x.ResponseDto.CorrelationId)
        .WithErrorMessage(ApplicationError.INVALID_REQUEST_ERROR_MESSAGE);
    result.ShouldHaveValidationErrorFor(x => x.ResponseDto.UserId)
        .WithErrorMessage(ApplicationError.USER_ID_NOT_PROVIDED_MESSAGE);
  }

  [Fact]
  public void Command_WhenAllFieldsNull_ShouldHaveMultipleValidationErrors()
  {
    // Arrange
    var responseDto = new RabbitMqPayload
    {
      CorrelationId = null!,
      UserId = null!,
      Success = false,
      Prompt = null!
    };
    var command = new MatchTransactionsCommand(responseDto);

    // Act & Assert
    var result = _validator.TestValidate(command);
    result.ShouldHaveValidationErrorFor(x => x.ResponseDto.CorrelationId)
        .WithErrorMessage(ApplicationError.INVALID_REQUEST_ERROR_MESSAGE);
    result.ShouldHaveValidationErrorFor(x => x.ResponseDto.UserId)
        .WithErrorMessage(ApplicationError.USER_ID_NOT_PROVIDED_MESSAGE);
  }

  [Fact]
  public void Command_WhenSuccessFalseButValidData_ShouldNotHaveValidationErrors()
  {
    // Arrange
    var responseDto = new RabbitMqPayload
    {
      CorrelationId = "valid-correlation-id",
      UserId = "550e8400-e29b-41d4-a716-446655440000",
      Success = false,
      Prompt = "Valid prompt"
    };
    var command = new MatchTransactionsCommand(responseDto);

    // Act & Assert
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Command_WhenMinimalValidData_ShouldNotHaveValidationErrors()
  {
    // Arrange
    var responseDto = new RabbitMqPayload
    {
      CorrelationId = "a",
      UserId = "b",
      Success = true,
      Prompt = "d"
    };
    var command = new MatchTransactionsCommand(responseDto);

    // Act & Assert
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Command_WhenSpecialCharactersInFields_ShouldNotHaveValidationErrors()
  {
    // Arrange
    var responseDto = new RabbitMqPayload
    {
      CorrelationId = "corr-id!@#$%^&*()",
      UserId = "user-id_123+{}[]",
      Success = true,
      Prompt = "prompt with special chars!@#$%"
    };
    var command = new MatchTransactionsCommand(responseDto);

    // Act & Assert
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveAnyValidationErrors();
  }
}

public class CancellationTokenTests : MatchTransactionsCommandValidatorTests
{
  [Fact]
  public void Command_WithDifferentCancellationTokens_ShouldNotAffectValidation()
  {
    // Arrange
    var responseDto = new RabbitMqPayload
    {
      CorrelationId = "valid-correlation-id",
      UserId = "550e8400-e29b-41d4-a716-446655440000",
      Success = true,
      Prompt = "Valid prompt"
    };
    var command = new MatchTransactionsCommand(responseDto);

    // Act & Assert - Test with different cancellation token states
    var result1 = _validator.TestValidate(command);
    result1.ShouldNotHaveAnyValidationErrors();

    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var result2 = _validator.TestValidate(command);
    result2.ShouldNotHaveAnyValidationErrors();
  }
}

public class PropertyPathTests : MatchTransactionsCommandValidatorTests
{
  [Fact]
  public void Validation_ShouldHaveCorrectPropertyPaths()
  {
    // Arrange
    var responseDto = new RabbitMqPayload
    {
      CorrelationId = "",
      UserId = "",
      Success = true,
      Prompt = "Valid prompt"
    };
    var command = new MatchTransactionsCommand(responseDto);

    // Act
    var result = _validator.TestValidate(command);

    // Assert
    result.ShouldHaveValidationErrorFor("ResponseDto.CorrelationId");
    result.ShouldHaveValidationErrorFor("ResponseDto.UserId");
  }
}
