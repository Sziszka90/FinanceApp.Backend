using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.UserApi.UserQueries.GetActiveUser;

public class GetActiveUserQueryHandler : IQueryHandler<GetActiveUserQuery, Result<GetUserDto>>
{
  private readonly ILogger<GetActiveUserQueryHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IUserService _userService;
  public GetActiveUserQueryHandler(
    ILogger<GetActiveUserQueryHandler> logger,
    IMapper mapper,
    IUserService userService)
  {
    _logger = logger;
    _mapper = mapper;
    _userService = userService;
  }

  public async Task<Result<GetUserDto>> Handle(GetActiveUserQuery request, CancellationToken cancellationToken)
  {
    var user = await _userService.GetActiveUserAsync(cancellationToken);

    if (!user.IsSuccess)
    {
      _logger.LogError("Failed to retrieve active user: {Error}", user.ApplicationError?.Message);
      return Result.Failure<GetUserDto>(user.ApplicationError!);
    }

    return Result.Success(_mapper.Map<GetUserDto>(user.Data!));
  }
}
