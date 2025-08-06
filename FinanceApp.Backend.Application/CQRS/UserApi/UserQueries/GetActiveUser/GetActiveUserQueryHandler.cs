using System.Security.Claims;
using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.QueryCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.UserApi.UserQueries.GetActiveUser;

public class GetActiveUserQueryHandler : IQueryHandler<GetActiveUserQuery, Result<GetUserDto>>
{
  private readonly ILogger<GetActiveUserQueryHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly IHttpContextAccessor _httpContextAccessor;
  public GetActiveUserQueryHandler(
    ILogger<GetActiveUserQueryHandler> logger,
    IMapper mapper,
    IRepository<Domain.Entities.User> userRepository,
    IHttpContextAccessor httpContextAccessor)
  {
    _logger = logger;
    _mapper = mapper;
    _userRepository = userRepository;
    _httpContextAccessor = httpContextAccessor;
  }

  public async Task<Result<GetUserDto>> Handle(GetActiveUserQuery request, CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    var criteria = UserQueryCriteria.FindUserEmail(userEmail!);

    var user = await _userRepository.GetQueryAsync(criteria, noTracking: true, cancellationToken: cancellationToken);

    if (user is null || user.Count == 0)
    {
      _logger.LogError("User not found with email:{Email}", userEmail);
      return Result.Failure<GetUserDto>(ApplicationError.UserNotFoundError(email: userEmail!));
    }

    _logger.LogInformation("Retrieved user with email:{Email}", userEmail);

    return Result.Success(_mapper.Map<GetUserDto>(user[0]));
  }
}
