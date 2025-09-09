using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UploadCsv;
using FluentValidation.TestHelper;

namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

public class LLMProcessorCommandValidatorTests : ValidatorTestBase
{
  private readonly LLMProcessorCommandValidator _validator;

  public LLMProcessorCommandValidatorTests()
  {
    _validator = new LLMProcessorCommandValidator();
  }

  public class ValidCommandTests : LLMProcessorCommandValidatorTests
  {
    [Fact]
    public void ValidCommand_ShouldNotHaveValidationErrors()
    {
      // Arrange
      var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
      {
        CorrelationId = "valid-correlation-id-123",
        UserId = "550e8400-e29b-41d4-a716-446655440000",
        Response = new MatchTransactionResponseDto
        {
          Transactions = new Dictionary<string, string>
          {
            { "Coffee", "Food" },
            { "Lunch", "Food" },
            { "Train ticket", "Transport" }
          }
        },
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new LLMProcessorCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidCommand_WithLongValues_ShouldNotHaveValidationErrors()
    {
      // Arrange
      var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
      {
        CorrelationId = CreateStringOfLength(100),
        UserId = "6ba7b810-9dad-11d1-80b4-00c04fd430c8",
        Response = new MatchTransactionResponseDto
        {
          Transactions = new Dictionary<string, string>
          {
            { "Coffee", "Food" },
            { "Lunch", "Food" },
            { "Train ticket", "Transport" }
          }
        },
        Success = false,
        Prompt = CreateStringOfLength(500)
      };
      var command = new LLMProcessorCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveAnyValidationErrors();
    }
  }

  public class CorrelationIdValidationTests : LLMProcessorCommandValidatorTests
  {
    [Fact]
    public void CorrelationId_WhenEmpty_ShouldHaveValidationError()
    {
      // Arrange
      var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
      {
        CorrelationId = string.Empty,
        UserId = "550e8400-e29b-41d4-a716-446655440000",
        Response = new MatchTransactionResponseDto
        {
          Transactions = new Dictionary<string, string>()
        },
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new LLMProcessorCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.ResponseDto.CorrelationId)
          .WithErrorMessage(ApplicationError.INVALID_REQUEST_ERROR_MESSAGE);
    }

    [Fact]
    public void CorrelationId_WhenNull_ShouldHaveValidationError()
    {
      // Arrange
      var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
      {
        CorrelationId = null!,
        UserId = "550e8400-e29b-41d4-a716-446655440000",
        Response = new MatchTransactionResponseDto
        {
          Transactions = new Dictionary<string, string>()
        },
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new LLMProcessorCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.ResponseDto.CorrelationId)
          .WithErrorMessage(ApplicationError.INVALID_REQUEST_ERROR_MESSAGE);
    }

    [Fact]
    public void CorrelationId_WhenWhitespace_ShouldHaveValidationError()
    {
      // Arrange
      var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
      {
        CorrelationId = "   ",
        UserId = "550e8400-e29b-41d4-a716-446655440000",
        Response = new MatchTransactionResponseDto
        {
          Transactions = new Dictionary<string, string>()
        },
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new LLMProcessorCommand(responseDto);

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
      var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
      {
        CorrelationId = correlationId,
        UserId = "550e8400-e29b-41d4-a716-446655440000",
        Response = new MatchTransactionResponseDto
        {
          Transactions = new Dictionary<string, string>
          {
            { "Coffee", "Food" },
            { "Lunch", "Food" },
            { "Train ticket", "Transport" }
          }
        },
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new LLMProcessorCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.ResponseDto.CorrelationId);
    }

    [Fact]
    public void CorrelationId_WhenVeryLong_ShouldNotHaveValidationError()
    {
      // Arrange
      var longCorrelationId = CreateStringOfLength(500);
      var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
      {
        CorrelationId = longCorrelationId,
        UserId = "550e8400-e29b-41d4-a716-446655440000",
        Response = new MatchTransactionResponseDto
        {
          Transactions = new Dictionary<string, string>
          {
            { "Coffee", "Food" },
            { "Lunch", "Food" },
            { "Train ticket", "Transport" }
          }
        },
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new LLMProcessorCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.ResponseDto.CorrelationId);
    }
  }

  public class UserIdValidationTests : LLMProcessorCommandValidatorTests
  {
    [Fact]
    public void UserId_WhenEmpty_ShouldHaveValidationError()
    {
      // Arrange
      var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
      {
        CorrelationId = "valid-correlation-id",
        UserId = string.Empty,
        Response = new MatchTransactionResponseDto
        {
          Transactions = new Dictionary<string, string>()
        },
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new LLMProcessorCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.ResponseDto.UserId)
          .WithErrorMessage(ApplicationError.USER_ID_NOT_PROVIDED_MESSAGE);
    }

    [Fact]
    public void UserId_WhenNull_ShouldHaveValidationError()
    {
      // Arrange
      var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
      {
        CorrelationId = "valid-correlation-id",
        UserId = null!,
        Response = new MatchTransactionResponseDto
        {
          Transactions = new Dictionary<string, string>()
        },
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new LLMProcessorCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldHaveValidationErrorFor(x => x.ResponseDto.UserId)
          .WithErrorMessage(ApplicationError.USER_ID_NOT_PROVIDED_MESSAGE);
    }

    [Fact]
    public void UserId_WhenWhitespace_ShouldHaveValidationError()
    {
      // Arrange
      var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
      {
        CorrelationId = "valid-correlation-id",
        UserId = "   ",
        Response = new MatchTransactionResponseDto
        {
          Transactions = new Dictionary<string, string>()
        },
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new LLMProcessorCommand(responseDto);

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
      var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
      {
        CorrelationId = "valid-correlation-id",
        UserId = userId,
        Response = new MatchTransactionResponseDto
        {
          Transactions = new Dictionary<string, string>
          {
            { "Coffee", "Food" }
          }
        },
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new LLMProcessorCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.ResponseDto.UserId);
    }

    [Fact]
    public void UserId_WhenLongString_ShouldNotHaveValidationError()
    {
      // Arrange
      var longUserId = CreateStringOfLength(100);
      var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
      {
        CorrelationId = "valid-correlation-id",
        UserId = longUserId,
        Response = new MatchTransactionResponseDto
        {
          Transactions = new Dictionary<string, string>
          {
            { "Coffee", "Food" }
          }
        },
        Success = true,
        Prompt = "Valid prompt"
      };
      var command = new LLMProcessorCommand(responseDto);

      // Act & Assert
      var result = _validator.TestValidate(command);
      result.ShouldNotHaveValidationErrorFor(x => x.ResponseDto.UserId);
    }
  }

  [Fact]
  public void Response_WhenLargeJsonResponse_ShouldNotHaveValidationError()
  {
    // Arrange
    var largeResponse = CreateStringOfLength(5000);
    var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
    {
      CorrelationId = "valid-correlation-id",
      UserId = "550e8400-e29b-41d4-a716-446655440000",
      Response = new MatchTransactionResponseDto
      {
        Transactions = new Dictionary<string, string>
          {
            { "LargeTransaction", largeResponse }
          }
      },
      Success = true,
      Prompt = "Valid prompt"
    };
    var command = new LLMProcessorCommand(responseDto);

    // Act & Assert
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveValidationErrorFor(x => x.ResponseDto.Response);
  }

  [Fact]
  public void Response_WhenComplexJsonData_ShouldNotHaveValidationError()
  {
    // Arrange
    var complexJsonResponse = "[{\"transactionId\":\"123\",\"groupId\":\"456\",\"confidence\":0.95},{\"transactionId\":\"789\",\"groupId\":\"101\",\"confidence\":0.87}]";
    var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
    {
      CorrelationId = "valid-correlation-id",
      UserId = "550e8400-e29b-41d4-a716-446655440000",
      Response = new MatchTransactionResponseDto
      {
        Transactions = new Dictionary<string, string>
          {
            { "ComplexTransaction", complexJsonResponse }
          }
      },
      Success = true,
      Prompt = "Valid prompt"
    };
    var command = new LLMProcessorCommand(responseDto);

    // Act & Assert
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveValidationErrorFor(x => x.ResponseDto.Response);
  }
}

public class EdgeCaseTests : LLMProcessorCommandValidatorTests
{
  [Fact]
  public void Command_WhenAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
  {
    // Arrange
    var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
    {
      CorrelationId = string.Empty,
      UserId = string.Empty,
      Response = new MatchTransactionResponseDto
      {
        Transactions = new Dictionary<string, string>()
      },
      Success = true,
      Prompt = "Valid prompt"
    };
    var command = new LLMProcessorCommand(responseDto);

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
    var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
    {
      CorrelationId = null!,
      UserId = null!,
      Response = null!,
      Success = false,
      Prompt = null!
    };
    var command = new LLMProcessorCommand(responseDto);

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
    var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
    {
      CorrelationId = "valid-correlation-id",
      UserId = "550e8400-e29b-41d4-a716-446655440000",
      Response = new MatchTransactionResponseDto
      {
        Transactions = new Dictionary<string, string>
          {
            { "ErrorTransaction", "Error occurred during processing" }
          }
      },
      Success = false,
      Prompt = "Valid prompt"
    };
    var command = new LLMProcessorCommand(responseDto);

    // Act & Assert
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Command_WhenMinimalValidData_ShouldNotHaveValidationErrors()
  {
    // Arrange
    var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
    {
      CorrelationId = "a",
      UserId = "b",
      Response = new MatchTransactionResponseDto
      {
        Transactions = new Dictionary<string, string>
          {
            { "Transaction1", "c" }
          }
      },
      Success = true,
      Prompt = "d"
    };
    var command = new LLMProcessorCommand(responseDto);

    // Act & Assert
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Command_WhenSpecialCharactersInFields_ShouldNotHaveValidationErrors()
  {
    // Arrange
    var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
    {
      CorrelationId = "corr-id!@#$%^&*()",
      UserId = "user-id_123+{}[]",
      Response = new MatchTransactionResponseDto
      {
        Transactions = new Dictionary<string, string>
          {
            { "special", "chars!@#$%^&*()" }
          }
      },
      Success = true,
      Prompt = "prompt with special chars!@#$%"
    };
    var command = new LLMProcessorCommand(responseDto);

    // Act & Assert
    var result = _validator.TestValidate(command);
    result.ShouldNotHaveAnyValidationErrors();
  }
}

public class CancellationTokenTests : LLMProcessorCommandValidatorTests
{
  [Fact]
  public void Command_WithDifferentCancellationTokens_ShouldNotAffectValidation()
  {
    // Arrange
    var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
    {
      CorrelationId = "valid-correlation-id",
      UserId = "550e8400-e29b-41d4-a716-446655440000",
      Response = new MatchTransactionResponseDto
      {
        Transactions = new Dictionary<string, string>
          {
            { "Transaction1", "Valid response" }
          }
      },
      Success = true,
      Prompt = "Valid prompt"
    };
    var command = new LLMProcessorCommand(responseDto);

    // Act & Assert - Test with different cancellation token states
    var result1 = _validator.TestValidate(command);
    result1.ShouldNotHaveAnyValidationErrors();

    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var result2 = _validator.TestValidate(command);
    result2.ShouldNotHaveAnyValidationErrors();
  }
}

public class PropertyPathTests : LLMProcessorCommandValidatorTests
{
  [Fact]
  public void Validation_ShouldHaveCorrectPropertyPaths()
  {
    // Arrange
    var responseDto = new RabbitMqPayload<MatchTransactionResponseDto>
    {
      CorrelationId = "",
      UserId = "",
      Response = new MatchTransactionResponseDto
      {
        Transactions = new Dictionary<string, string>()
      },
      Success = true,
      Prompt = "Valid prompt"
    };
    var command = new LLMProcessorCommand(responseDto);

    // Act
    var result = _validator.TestValidate(command);

    // Assert
    result.ShouldHaveValidationErrorFor("ResponseDto.CorrelationId");
    result.ShouldHaveValidationErrorFor("ResponseDto.UserId");
  }
}
}
