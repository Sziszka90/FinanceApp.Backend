using System.Security.Claims;
using FinanceApp.Application.Abstraction.Services;
using Microsoft.AspNetCore.Http;

namespace FinanceApp.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    public string? UserName { get; }

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        UserName = accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}