using FinanceApp.Backend.Application.Extensions;
using FluentValidation.Results;
using OpenAI.VectorStores;

namespace FinanceApp.Backend.Application.Models;

public class ApplicationError
{

  public const string TOKEN_NOT_PROVIDED_MESSAGE = "Token not provided.";
  public const string USER_ID_NOT_PROVIDED_MESSAGE = "User ID not provided.";
  public const string INVALID_REQUEST_ERROR_MESSAGE = "Invalid request.";
  public const string INVALID_EMAIL_FORMAT = "Invalid email format.";
  public const string FIELD_CANNOT_BE_EMPTY = "Field cannot be empty.";

  public const string DEFAULT_MESSAGE = "An exception occurred.";
  public const string DEFAULT_CODE = "EXCEPTION_OCCURRED";

  public const string ENTITYNOTFOUND_MESSAGE = "Entity not found.";
  public const string ENTITYNOTFOUND_CODE = "ENTITY_NOT_FOUND";

  public const string USERNOTFOUND_MESSAGE = "User not found.";
  public const string USERNOTFOUND_CODE = "USER_NOT_FOUND";

  public const string INVALID_PASSWORD_MESSAGE = "Invalid password";
  public const string INVALID_PASSWORD_CODE = "INVALID_PASSWORD";

  public const string INVALID_TOKEN_MESSAGE = "Invalid token";
  public const string INVALID_TOKEN_CODE = "INVALID_TOKEN";

  public const string INVALID_EXCHANGE_RATE_RESPONSE_MESSAGE = "Invalid exchange rate response";
  public const string INVALID_EXCHANGE_RATE_RESPONSE_CODE = "INVALID_EXCHANGE_RATE_RESPONSE";

  public const string MISSING_EXCHANGE_RATES_MESSAGE = "Missing exchange rates.";
  public const string MISSING_EXCHANGE_RATES_CODE = "MISSING_EXCHANGE_RATES";

  public const string NAME_ALREADY_EXISTS_MESSAGE = "Entity with this name already exists.";
  public const string NAME_ALREADY_EXISTS_CODE = "NAME_ALREADY_EXISTS";

  public const string USERNAME_ALREADY_EXISTS_MESSAGE = "User with this name already exists.";
  public const string USERNAME_ALREADY_EXISTS_CODE = "USERNAME_ALREADY_EXISTS";

  public const string USERNAME_NOT_LOGGED_IN_MESSAGE = "User is not logged in.";
  public const string USERNAME_NOT_LOGGED_IN_CODE = "USERNAME_NOT_LOGGED_IN";

  public const string EMAIL_NOT_YET_CONFIRMED_MESSAGE = "Email address not yet confirmed.";
  public const string EMAIL_NOT_YET_CONFIRMED_CODE = "EMAIL_NOT_YET_CONFIRMED";

  public const string USEREMAIL_ALREADY_EXISTS_MESSAGE = "User with this email already exists.";
  public const string USEREMAIL_ALREADY_EXISTS_CODE = "USEREMAIL_ALREADY_EXISTS";

  public const string USEREMAIL_CONFIRMATION_ERROR_MESSAGE = "Email confirmation error.";
  public const string USEREMAIL_CONFIRMATION_ERROR_CODE = "USEREMAIL_CONFIRMATION_ERROR";

  public const string TRANSACTION_GROUP_NOT_EXISTS_MESSAGE = "Transaction group does not exists.";
  public const string TRANSACTION_GROUP_NOT_EXISTS_CODE = "TRANSACTION_GROUP_NOT_EXISTS";

  public const string TRANSACTION_ID_REQUIRED_MESSAGE = "Transaction ID is required.";
  public const string TRANSACTION_ID_REQUIRED_CODE = "TRANSACTION_ID_REQUIRED";

  public const string DBUPDATEERROR_MESSAGE = "Error while saving changes.";
  public const string DBUPDATEERROR_CODE = "DB_UPDATE_ERROR";

  public const string DBNULLERROR_MESSAGE = "Cannot insert null.";
  public const string DBNULLERROR_CODE = "DB_CANNOT_INSERT_NULL";

  public const string DBCONSTRAINTERROR_MESSAGE = "DB constraint violation";
  public const string DBCONSTRAINTERROR_CODE = "DB_CONSTRAINT_VIOLATION";

  public const string DBCONNERR_MESSAGE = "DB connection error";
  public const string DBCONNERR_CODE = "DB_CONNECTION_ERROR";

  public const string EXT_CALL_MESSAGE = "External call error";
  public const string EXT_CALL_CODE = "EXT_CALL_ERROR";

  public const string VALIDATION_MESSAGE = "Validation failed.";
  public const string VALIDATION_CODE = "VALIDATION_FAILED";

  public const string FILE_EMPTY_ERROR_MESSAGE = "File is empty.";
  public const string FILE_EMPTY_ERROR_CODE = "FILE_EMPTY";

  public const string INVALID_FILE_TYPE_ERROR_MESSAGE = "Invalid file type.";
  public const string INVALID_FILE_TYPE_ERROR_CODE = "INVALID_FILE_TYPE";

  public const string UNKNOWN_TOKEN_TYPE_MESSAGE = "Unknown token type.";
  public const string UNKNOWN_TOKEN_TYPE_CODE = "UNKNOWN_TOKEN_TYPE";

  public const string PARSING_ERROR_MESSAGE = "Parsing error.";
  public const string PARSING_ERROR_CODE = "PARSING_ERROR";

  public const string TRANSACTION_GROUP_IS_USED_MESSAGE = "Transaction group is used by transactions.";
  public const string TRANSACTION_GROUP_IS_USED_CODE = "TRANSACTION_GROUP_IS_USED";

  public const string LLM_PROCESSOR_REQUEST_MESSAGE = "LLM Processor request error.";
  public const string LLM_PROCESSOR_REQUEST_CODE = "LLM_PROCESSOR_REQUEST_ERROR";

  public const string TOKEN_EXPIRED_MESSAGE = "Token is expired.";
  public const string TOKEN_EXPIRED_CODE = "TOKEN_EXPIRED";

  public const string EMAIL_NOT_FOUND_IN_TOKEN_MESSAGE = "Email not found in token.";
  public const string EMAIL_NOT_FOUND_IN_TOKEN_CODE = "EMAIL_NOT_FOUND_IN_TOKEN";

  public const string TOKEN_GENERATION_ERROR_MESSAGE = "Token generation error.";
  public const string TOKEN_GENERATION_ERROR_CODE = "TOKEN_GENERATION_ERROR";

  public const string EMAIL_TEMPLATE_NOT_FOUND_MESSAGE = "Email template not found.";
  public const string EMAIL_TEMPLATE_NOT_FOUND_CODE = "EMAIL_TEMPLATE_NOT_FOUND";

  public const string CACHECONNERR_MESSAGE = "Cache connection error";
  public const string CACHECONNERR_CODE = "CACHE_CONNECTION_ERROR";

  /// <summary>
  /// Machine readable error code
  /// </summary>
  public string Code { get; } = string.Empty;

  /// <summary>
  /// Human readable error message
  /// </summary>
  public string Message { get; } = string.Empty;

  /// <summary>
  /// Relative source path on which the error occurred.
  /// </summary>
  public string Path { get; set; } = string.Empty;

  /// <summary>
  /// Detailed error information excluding any security relevant information.
  /// </summary>
  public Dictionary<string, object> Details { get; set; } = new();

  public ApplicationError() { }

  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="message"></param>
  /// <param name="code"></param>
  /// <param name="details"></param>
  /// <param name="path"></param>
  public ApplicationError(string message, string code, Dictionary<string, object>? details = null, string path = "")
  {
    Message = message;
    Code = code;
    Details = details ?? new Dictionary<string, object>();
    Path = path;
  }

  /// <summary>
  /// Default error for when we receive an exception
  /// <param name="exceptionMessage"></param>
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError DefaultError(string exceptionMessage)
  {
    return new ApplicationError(DEFAULT_MESSAGE, DEFAULT_CODE, new Dictionary<string, object>
    {
      { "message", exceptionMessage }
    });
  }

  /// <summary>
  /// Error for when writing to DB fails
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError DbUpdateError()
  {
    return new ApplicationError(DBUPDATEERROR_MESSAGE, DBUPDATEERROR_CODE);
  }

  /// <summary>
  /// Error for when an entity is not found
  /// </summary>
  /// <param name="entityId"></param>
  /// <param name="entityType"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError EntityNotFoundError(string entityId = "", string entityType = "")
  {
    return new ApplicationError(ENTITYNOTFOUND_MESSAGE, ENTITYNOTFOUND_CODE, new Dictionary<string, object>
    {
      { "entityType", entityType },
      { "identifier", entityId }
    });
  }

  /// <summary>
  /// Error for when a user is not found
  /// </summary>
  /// <param name="userId"></param>
  /// <param name="userName"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError UserNotFoundError(string userId = "", string email = "")
  {
    return new ApplicationError(USERNOTFOUND_MESSAGE, USERNOTFOUND_CODE, new Dictionary<string, object>
    {
      { "userId", userId },
      { "email", email }
    });
  }

  /// <summary>
  /// User not logged in error
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError UserNotLoggedInError()
  {
    return new ApplicationError(USERNAME_NOT_LOGGED_IN_MESSAGE, USERNAME_NOT_LOGGED_IN_CODE);
  }

  /// <summary>
  /// Error for when password is invalid
  /// </summary>
  /// <param name="userName"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError InvalidPasswordError(string userName = "")
  {
    return new ApplicationError(INVALID_PASSWORD_MESSAGE, INVALID_PASSWORD_CODE, new Dictionary<string, object>
    {
      { "userName", userName }
    });
  }

  /// <summary>
  /// Error during email confirm validation
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError InvalidTokenError(string tokenType = "")
  {
    return new ApplicationError(INVALID_TOKEN_MESSAGE, INVALID_TOKEN_CODE, new Dictionary<string, object>
    {
      { "tokenType", tokenType }
    });
  }

  /// <summary>
  /// Error for when exchange rate response is invalid
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError UnknownTokenTypeError(string tokenType = "")
  {
    return new ApplicationError(UNKNOWN_TOKEN_TYPE_MESSAGE, UNKNOWN_TOKEN_TYPE_CODE, new Dictionary<string, object>
    {
      { "tokenType", tokenType }
    });
  }

  /// <summary>
  /// Error when a null value was inserted into a nun-nullable field
  /// </summary>
  /// <param name="fieldName"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError DbNullError(string fieldName)
  {
    return new ApplicationError(DBNULLERROR_MESSAGE, DBNULLERROR_CODE, new Dictionary<string, object>
    {
      { "fieldName", fieldName }
    });
  }

  /// <summary>
  /// Error for when name already exists
  /// </summary>
  /// <param name="name"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError NameAlreadyExistsError(string name)
  {
    return new ApplicationError(NAME_ALREADY_EXISTS_MESSAGE, NAME_ALREADY_EXISTS_CODE, new Dictionary<string, object>
    {
      { "name", name }
    });
  }

  /// <summary>
  /// Error for when user name already exists
  /// </summary>
  /// <param name="name"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError UserNameAlreadyExistsError(string name)
  {
    return new ApplicationError(USERNAME_ALREADY_EXISTS_MESSAGE, USERNAME_ALREADY_EXISTS_CODE, new Dictionary<string, object>
    {
      { "name", name }
    });
  }

  /// <summary>
  /// Email address not yet confirmed error
  /// </summary>
  /// <param name="email"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError EmailNotYetConfirmedError(string email)
  {
    return new ApplicationError(EMAIL_NOT_YET_CONFIRMED_MESSAGE, EMAIL_NOT_YET_CONFIRMED_CODE, new Dictionary<string, object>
    {
      { "email", email }
    });
  }

  /// <summary>
  /// Error for when user email already exists
  /// </summary>
  /// <param name="email"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError UserEmailAlreadyExistsError(string email)
  {
    return new ApplicationError(USEREMAIL_ALREADY_EXISTS_MESSAGE, USEREMAIL_ALREADY_EXISTS_CODE, new Dictionary<string, object>
    {
      { "email", email }
    });
  }

  /// <summary>
  /// Email confirmation error
  /// </summary>
  /// <param name="email"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError EmailConfirmationError(string email)
  {
    return new ApplicationError(USEREMAIL_CONFIRMATION_ERROR_MESSAGE, USEREMAIL_CONFIRMATION_ERROR_CODE, new Dictionary<string, object>
    {
      { "email", email }
    });
  }

  /// <summary>
  /// Error for when transaction group does not exists
  /// </summary>
  /// <param name="groupId"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError TransactionGroupNotExists(string groupId)
  {
    return new ApplicationError(TRANSACTION_GROUP_NOT_EXISTS_MESSAGE, TRANSACTION_GROUP_NOT_EXISTS_CODE, new Dictionary<string, object>
    {
      { "GroupId", groupId }
    });
  }

  /// <summary>
  /// Error when DB connection fails
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError DbConnectionError()
  {
    return new ApplicationError(DBCONNERR_MESSAGE, DBCONNERR_CODE);
  }

  /// <summary>
  /// External call error
  /// </summary>
  /// <param name="message"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError ExternalCallError(string message)
  {
    return new ApplicationError(EXT_CALL_MESSAGE, EXT_CALL_CODE, new Dictionary<string, object>
    {
      { "message", message }
    });
  }

  /// <summary>
  /// Error when Validation fails
  /// </summary>
  /// <param name="validationFailures"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError ValidationError(List<ValidationFailure> validationFailures)
  {
    // Catching null references.
    foreach (var validationFailure in validationFailures)
    {
      validationFailure.ErrorCode ??= VALIDATION_CODE;

      validationFailure.FormattedMessagePlaceholderValues ??= new Dictionary<string, object>
      {
        { "PropertyName", validationFailure.PropertyName }
      };
    }

    var details = validationFailures.DistinctBy(x => $"{x.ErrorCode}-{x.FormattedMessagePlaceholderValues["PropertyName"]}-{x.ErrorMessage}")
                                    .ToUniqueDictionaryWithCounters(x => $"{x.ErrorCode}_{x.FormattedMessagePlaceholderValues["PropertyName"]}", x => x.ErrorMessage as object);
    return new ApplicationError(VALIDATION_MESSAGE, VALIDATION_CODE, details);
  }

  /// <summary>
  /// Error for when file is empty
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError FileEmptyError()
  {
    return new ApplicationError(FILE_EMPTY_ERROR_MESSAGE, FILE_EMPTY_ERROR_CODE);
  }

  /// <summary>
  /// Error for when file type is invalid
  /// </summary>
  /// <param name="fileType"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError InvalidFileTypeError(string fileType)
  {
    return new ApplicationError(INVALID_FILE_TYPE_ERROR_MESSAGE, INVALID_FILE_TYPE_ERROR_CODE, new Dictionary<string, object>
    {
      { "fileType", fileType }
    });
  }

  /// <summary>
  /// Parsing error for when a file cannot be parsed
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError ParsingError()
  {
    return new ApplicationError(PARSING_ERROR_MESSAGE, PARSING_ERROR_CODE);
  }

  /// <summary>
  /// Error for when exchange rates are missing
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError MissingExchangeRatesError()
  {
    return new ApplicationError(MISSING_EXCHANGE_RATES_MESSAGE, MISSING_EXCHANGE_RATES_CODE);
  }

  /// <summary>
  /// Error when transaction group is used by transactions
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError TransactionGroupIsUsedError()
  {
    return new ApplicationError(TRANSACTION_GROUP_IS_USED_MESSAGE, TRANSACTION_GROUP_IS_USED_CODE);
  }

  /// <summary>
  /// Error during LLM Processor request
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError LLMProcessorRequestError(string message)
  {
    return new ApplicationError(LLM_PROCESSOR_REQUEST_MESSAGE, LLM_PROCESSOR_REQUEST_CODE, new Dictionary<string, object>
    {
      { "message", message }
    });
  }

  /// <summary>
  /// Token expired
  /// </summary>
  /// <param name="email"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError TokenExpiredError(string email)
  {
    return new ApplicationError(TOKEN_EXPIRED_MESSAGE, TOKEN_EXPIRED_CODE, new Dictionary<string, object>
    {
      { "email", email }
    });
  }

  /// <summary>
  /// Error for when email is not found in token
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError EmailNotFoundInTokenError()
  {
    return new ApplicationError(EMAIL_NOT_FOUND_IN_TOKEN_MESSAGE, EMAIL_NOT_FOUND_IN_TOKEN_CODE);
  }

  /// <summary>
  /// Error when cannot generate unique token
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError TokenGenerationError()
  {
    return new ApplicationError(TOKEN_GENERATION_ERROR_MESSAGE, TOKEN_GENERATION_ERROR_CODE);
  }

  /// <summary>
  /// Error for when email template is not found
  /// </summary>
  /// <param name="templateName"></param>
  /// <returns>ApplicationError</returns>
  public static ApplicationError EmailTemplateNotFoundError(string templateName)
  {
    return new ApplicationError(EMAIL_TEMPLATE_NOT_FOUND_MESSAGE, EMAIL_TEMPLATE_NOT_FOUND_CODE, new Dictionary<string, object>
    {
      { "templateName", templateName }
    });
  }

  /// <summary>
  /// Error when cache connection fails
  /// </summary>
  /// <returns>ApplicationError</returns>
  public static ApplicationError CacheConnectionError()
  {
    return new ApplicationError(CACHECONNERR_MESSAGE, CACHECONNERR_CODE);
  }
}
