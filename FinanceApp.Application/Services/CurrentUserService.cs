using System.Security.Claims;
using FinanceApp.Application.Abstraction.Services;
using Microsoft.AspNetCore.Http;

namespace FinanceApp.Application.Services;

public class CurrentUserService : ICurrentUserService
{
  private readonly IHttpContextAccessor? _contextAccessor;
  public string UserName { get; } = String.Empty;

  public CurrentUserService(IHttpContextAccessor? contextAccessor = null)
  {
    _contextAccessor = contextAccessor;

    if (_contextAccessor?.HttpContext is not null)
    {
      UserName = _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? String.Empty;
    }
  }
}
