using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.ResetToken;

public class ResetTokenCommandHandler : ICommandHandler<ResetTokenCommand, Result>
{
  private readonly ILogger<ResetTokenCommandHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public ResetTokenCommandHandler(
    ILogger<ResetTokenCommandHandler> logger,
    IHttpContextAccessor httpContextAccessor)
  {
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
  }

  public Task<Result> Handle(ResetTokenCommand request, CancellationToken cancellationToken)
  {
    var context = _httpContextAccessor.HttpContext;

    context?.Response.Cookies.Append("Token", "", new CookieOptions
    {
      Expires = DateTimeOffset.UtcNow.AddDays(-1),
      HttpOnly = true,
      Secure = true,
      SameSite = SameSiteMode.None,
      Path = "/",
      MaxAge = TimeSpan.Zero
    });

    context?.Response.Cookies.Append("RefreshToken", "", new CookieOptions
    {
      Expires = DateTimeOffset.UtcNow.AddDays(-1),
      HttpOnly = true,
      Secure = true,
      SameSite = SameSiteMode.None,
      Path = "/",
      MaxAge = TimeSpan.Zero
    });

    _logger.LogInformation("ResetTokenCommandHandler: Tokens reset in cookies successfully.");

    return Task.FromResult(Result.Success());
  }
}
