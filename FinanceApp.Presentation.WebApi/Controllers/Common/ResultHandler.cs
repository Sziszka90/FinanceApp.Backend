using FinanceApp.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Presentation.WebApi.Controllers.Common;

public static class ResultHandler
{
  /// <summary>
  /// Reads a Result and returns a ActionResult
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="controller"></param>
  /// <param name="appResult"></param>
  /// <param name="succeededStatusCode">
  /// Status Code to be use in case of succeeded application result - default is
  /// StatusCodes.Status200OK
  /// </param>
  /// <returns></returns>
  public static ActionResult GetResult<T>(this ControllerBase controller, Result<T> appResult, int succeededStatusCode = StatusCodes.Status200OK)
  {
    if (!appResult.IsSuccess)
    {
      return AssignHttpCodeToError(controller, appResult.ApplicationError!);
    }

    if (appResult.Data == null)
    {
      return controller.StatusCode(succeededStatusCode);
    }

    return controller.StatusCode(succeededStatusCode, appResult.Data);
  }

  /// <summary>
  /// Reads a Result and returns a ActionResult
  /// </summary>
  /// <param name="controller"></param>
  /// <param name="appResult"></param>
  /// <param name="succeededStatusCode">
  /// Status Code to be use in case of succeeded application result - default is
  /// StatusCodes.Status200OK
  /// </param>
  /// <returns></returns>
  public static ActionResult GetResult(this ControllerBase controller, Result appResult, int succeededStatusCode = StatusCodes.Status200OK)
  {
    return !appResult.IsSuccess ? AssignHttpCodeToError(controller, appResult.ApplicationError!) : controller.StatusCode(succeededStatusCode);
  }

  /// <summary>
  /// Returns an ActionResult with the assigned StatusCodes
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="controller"></param>
  /// <param name="error"></param>
  /// <returns></returns>
  private static ActionResult AssignHttpCodeToError(ControllerBase controller, ApplicationError error)
  {
    string path = controller.Request.PathBase + controller.Request.Path;

    var errorResult = new ErrorResult(error, path);

    return error.Code switch
    {
      ApplicationError.ENTITYNOTFOUND_CODE or ApplicationError.USERNOTFOUND_CODE => controller.NotFound(errorResult),
      ApplicationError.DBCONSTRAINTERROR_CODE => controller.BadRequest(errorResult),
      ApplicationError.VALIDATION_CODE => controller.BadRequest(errorResult),
      ApplicationError.NAME_ALREADY_EXISTS_CODE => controller.BadRequest(errorResult),
      ApplicationError.INVALID_PASSWORD_CODE => controller.Unauthorized(errorResult),
      _ => controller.StatusCode(StatusCodes.Status500InternalServerError, errorResult)
    };
  }
}
