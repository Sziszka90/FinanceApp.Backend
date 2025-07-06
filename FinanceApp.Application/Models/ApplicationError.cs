using FinanceApp.Application.Extensions;
using FluentValidation.Results;

namespace FinanceApp.Application.Models;

public class ApplicationError
{

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

  public const string NAME_ALREADY_EXISTS_MESSAGE = "Entity with this name already exists.";
  public const string NAME_ALREADY_EXISTS_CODE = "NAME_ALREADY_EXISTS";

  public const string USERNAME_ALREADY_EXISTS_MESSAGE = "User with this name already exists.";
  public const string USERNAME_ALREADY_EXISTS_CODE = "USERNAME_ALREADY_EXISTS";

  public const string EMAIL_NOT_YET_CONFIRMED_MESSAGE = "Email address not yet confirmed.";
  public const string EMAIL_NOT_YET_CONFIRMED_CODE = "EMAIL_NOT_YET_CONFIRMED";

  public const string USEREMAIL_ALREADY_EXISTS_MESSAGE = "User with this email already exists.";
  public const string USEREMAIL_ALREADY_EXISTS_CODE = "USEREMAIL_ALREADY_EXISTS";

  public const string USEREMAIL_CONFIRMATION_ERROR_MESSAGE = "Email confirmation error.";
  public const string USEREMAIL_CONFIRMATION_ERROR_CODE = "USEREMAIL_CONFIRMATION_ERROR";

  public const string TRANSACTION_GROUP_NOT_EXISTS_MESSAGE = "Transaction group does not exists.";
  public const string TRANSACTION_GROUP_NOT_EXISTS_CODE = "TRANSACTION_GROUP_NOT_EXISTS";

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

  public const string SALT_EDGE_USER_CREATION_MESSAGE = "Failed to create user in Salt Edge.";
  public const string SALT_EDGE_USER_CREATION_CODE = "SALT_EDGE_USER_CREATION_FAILED";

  public const string SALT_EDGE_USER_CONNECTION_MESSAGE = "Failed to create connection in Salt Edge.";
  public const string SALT_EDGE_USER_CONNECTION_CODE = "SALT_EDGE_USER_CONNECTION_FAILED";

  public const string SALT_EDGE_USER_MISSING_ID_MESSAGE = "Missing Salt Edge identifier for user.";
  public const string SALT_EDGE_USER_MISSING_ID_CODE = "SALT_EDGE_USER_MISSING_ID_FAILED";

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

  /// <summary>
  /// Error for when writing to DB fails
  /// </summary>
  public static ApplicationError DbUpdateError => new(DBUPDATEERROR_MESSAGE, DBUPDATEERROR_CODE);

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

  public ApplicationError() { }

  /// <summary>
  /// Default error for when we receive an exception
  /// </summary>
  public static ApplicationError DefaultError(string exceptionMessage)
  {
    return new ApplicationError(DEFAULT_MESSAGE, DEFAULT_CODE, new Dictionary<string, object>
    {
      { "message", exceptionMessage }
    });
  }

  /// <summary>
  /// Error for when an entity is not found
  /// </summary>
  /// <returns></returns>
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
  /// <returns></returns>
  public static ApplicationError UserNotFoundError(string userId = "", string userName = "")
  {
    return new ApplicationError(USERNOTFOUND_MESSAGE, USERNOTFOUND_CODE, new Dictionary<string, object>
    {
      { "userName", userName },
      { "identifier", userId }
    });
  }

  /// <summary>
  /// Error for when password is invalid
  /// </summary>
  /// <returns></returns>
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
  /// <returns></returns>
  public static ApplicationError InvalidTokenError()
  {
    return new ApplicationError(INVALID_TOKEN_MESSAGE, INVALID_TOKEN_CODE);
  }

  /// <summary>
  /// Error for when exchange rate response is invalid
  /// </summary>
  /// <returns></returns>
  public static ApplicationError InvalidExchangeRateResponseError()
  {
    return new ApplicationError(INVALID_EXCHANGE_RATE_RESPONSE_MESSAGE, INVALID_EXCHANGE_RATE_RESPONSE_CODE, new Dictionary<string, object>
    {
      { "message", "The exchange rate response is invalid or cannot be parsed." }
    });
  }

  /// <summary>
  /// Error when a null value was inserted into a nun-nullable field
  /// </summary>
  /// <param name="fieldName"></param>
  /// <returns></returns>
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
  /// <returns></returns>
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
  /// <returns></returns>
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
  /// <returns></returns>
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
  /// <returns></returns>
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
  /// <returns></returns>
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
  /// <returns></returns>
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
  /// <returns></returns>
  public static ApplicationError DbConnectionError()
  {
    return new ApplicationError(DBCONNERR_MESSAGE, DBCONNERR_CODE);
  }

  /// <summary>
  /// External call error
  /// </summary>
  /// <returns></returns>
  public static ApplicationError ExternalCallError()
  {
    return new ApplicationError(DBCONNERR_MESSAGE, DBCONNERR_CODE);
  }

  /// <summary>
  /// Salt Edge user creation error
  /// </summary>
  /// <returns></returns>
  public static ApplicationError SaltEdgeUserCreationError(string email)
  {
    return new ApplicationError(SALT_EDGE_USER_CREATION_MESSAGE, SALT_EDGE_USER_CREATION_CODE, new Dictionary<string, object>
    {
      { "email", email }
    });
  }

  /// <summary>
  /// Salt Edge user connection error
  /// </summary>
  /// <returns></returns>
  public static ApplicationError SaltEdgeUserConnectionError()
  {
    return new ApplicationError(SALT_EDGE_USER_CONNECTION_MESSAGE, SALT_EDGE_USER_CONNECTION_CODE);
  }

  /// <summary>
  /// Missing Salt Edge identifier error
  /// </summary>
  /// <param name="constraintName"></param>
  /// <param name="tableName"></param>
  /// <returns></returns>
  public static ApplicationError MissingSaltEdgeIdentifierError(string email)
  {
    return new ApplicationError(SALT_EDGE_USER_MISSING_ID_MESSAGE, SALT_EDGE_USER_MISSING_ID_CODE, new Dictionary<string, object>
    {
      { "Email", email }
    });
  }

  /// <summary>
  /// Error when Validation fails
  /// </summary>
  /// <returns></returns>
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
}
