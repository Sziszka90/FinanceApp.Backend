using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Backend.Application.Exceptions;

namespace FinanceApp.Backend.Presentation.WebApi.Middlewares;

public class ExceptionHandlingMiddleware
{
  private readonly ILogger<ExceptionHandlingMiddleware> _logger;
  private readonly RequestDelegate _next;

  public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
  {
    _logger = logger;
    _next = next;
  }

  public async Task Invoke(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unhandled exception occurred");
      await HandleExceptionAsync(context, ex);
    }
  }

  private static Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    var code = exception switch
    {
      ValidationException => StatusCodes.Status400BadRequest,
      DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
      DbUpdateException => StatusCodes.Status500InternalServerError,
      KeyNotFoundException => StatusCodes.Status404NotFound,
      ArgumentNullException => StatusCodes.Status400BadRequest,
      ArgumentException => StatusCodes.Status400BadRequest,
      UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
      CacheException => StatusCodes.Status503ServiceUnavailable,
      DatabaseException => StatusCodes.Status500InternalServerError,
      HttpClientException => StatusCodes.Status500InternalServerError,
      RabbitMqException => StatusCodes.Status503ServiceUnavailable,
      SignalRException => StatusCodes.Status500InternalServerError,
      _ => StatusCodes.Status500InternalServerError
    };

    var result = JsonSerializer.Serialize(new
    {
      error = exception.Message,
      type = exception.GetType().Name
    });

    context.Response.ContentType = "application/json";
    context.Response.StatusCode = code;

    return context.Response.WriteAsync(result);
  }
}
