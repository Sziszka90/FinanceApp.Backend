using System.Security.Claims;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.QueryCriteria;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.Abstraction.Services;

public class UserService : IUserService
{
  private readonly ILogger<UserService> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IUserRepository _userRepository;

  public UserService(
    ILogger<UserService> logger,
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository)
  {
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
    _userRepository = userRepository;
  }

  public async Task<Result<User>> GetActiveUserAsync(CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;
    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userEmail))
    {
      _logger.LogError("User email not found in claims.");
      return Result.Failure<User>(ApplicationError.UserNotLoggedInError());
    }

    var criteria = UserQueryCriteria.FindUserEmail(userEmail!);
    var user = await _userRepository.GetQueryAsync(criteria);

    if (user is null || user.Count == 0)
    {
      _logger.LogError("User not found with email: {Email}", userEmail);
      return Result.Failure<User>(ApplicationError.UserNotFoundError(email: userEmail!));
    }

    _logger.LogInformation("Retrieved user with email:{Email}", userEmail);

    return Result.Success(user.FirstOrDefault()!);
  }

  public Result<string> GetActiveUserToken()
  {
    var httpContext = _httpContextAccessor.HttpContext;
    var token = httpContext!.Request.Headers.Authorization.FirstOrDefault();

    if (string.IsNullOrEmpty(token))
    {
      _logger.LogError("Authorization token not found.");
      return Result.Failure<string>(ApplicationError.InvalidTokenError());
    }
    return Result.Success(token);
  }
}
