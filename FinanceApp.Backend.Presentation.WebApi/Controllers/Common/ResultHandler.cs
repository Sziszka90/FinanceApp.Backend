using FinanceApp.Backend.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Backend.Presentation.WebApi.Controllers.Common;

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

    if (succeededStatusCode == StatusCodes.Status302Found)
    {
      if (!appResult.IsSuccess)
      {
        controller.Response.Headers.Location = $"https://www.financeapp.fun/validation-failed";
        return controller.StatusCode(succeededStatusCode);
      }
      controller.Response.Headers.Location = "https://www.financeapp.fun/login";
      return controller.StatusCode(succeededStatusCode);
    }

    if (!appResult.IsSuccess)
    {
      return AssignHttpCodeToError(controller, appResult.ApplicationError!);
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
  /// Redirect a request to a specific URL
  /// </summary>
  /// <param name="controller"></param>
  /// </summary>
  public static ActionResult RedirectToUrl(this ControllerBase controller, Result appResult, string url)
  {
    if (!appResult.IsSuccess)
    {
      controller.Response.Headers.Location = $"https://www.financeapp.fun/validation-failed";
      return controller.StatusCode(StatusCodes.Status302Found);
    }
    controller.Response.Headers.Location = url;
    return controller.StatusCode(StatusCodes.Status302Found);
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

      ApplicationError.ENTITYNOTFOUND_CODE or
      ApplicationError.USERNOTFOUND_CODE or
      ApplicationError.TRANSACTION_GROUP_NOT_EXISTS_CODE => controller.NotFound(errorResult),
      ApplicationError.NAME_ALREADY_EXISTS_CODE or
      ApplicationError.USEREMAIL_ALREADY_EXISTS_CODE or
      ApplicationError.VALIDATION_CODE or
      ApplicationError.DBCONSTRAINTERROR_CODE or
      ApplicationError.USERNAME_ALREADY_EXISTS_CODE => controller.BadRequest(errorResult),
      ApplicationError.INVALID_PASSWORD_CODE or
      ApplicationError.INVALID_TOKEN_CODE => controller.Unauthorized(errorResult),
      _ => controller.StatusCode(StatusCodes.Status500InternalServerError, errorResult)
    };
  }
}
