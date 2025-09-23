using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.SetToken;

public class SetTokenCommandHandler : ICommandHandler<SetTokenCommand, Result>
{
  private readonly ILogger<SetTokenCommandHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public SetTokenCommandHandler(
    ILogger<SetTokenCommandHandler> logger,
    IHttpContextAccessor httpContextAccessor)
  {
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
  }

  public Task<Result> Handle(SetTokenCommand request, CancellationToken cancellationToken)
  {
    var context = _httpContextAccessor.HttpContext;
    context?.Response.Cookies.Append("Token", request.Token, new CookieOptions
    {
      HttpOnly = true,
      Secure = true,
      SameSite = SameSiteMode.None
    });

    context?.Response.Cookies.Append("RefreshToken", request.RefreshToken, new CookieOptions
    {
      HttpOnly = true,
      Secure = true,
      SameSite = SameSiteMode.None
    });

    _logger.LogInformation("SetTokenCommandHandler: Tokens set in cookies successfully.");

    return Task.FromResult(Result.Success());
  }
}
