using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using FinanceApp.Application.User.UserQueries;
using Microsoft.AspNetCore.Http;

namespace FinanceApp.User.UserQueries.SavingQueries;

public class GetActiveUserQueryHandler : IQueryHandler<GetActiveUserQuery, Result<GetUserDto>>
{
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly IHttpContextAccessor _httpContextAccessor;
  public GetActiveUserQueryHandler(IMapper mapper, IRepository<Domain.Entities.User> userRepository, IHttpContextAccessor httpContextAccessor)
  {
    _mapper = mapper;
    _userRepository = userRepository;
    _httpContextAccessor = httpContextAccessor;
  }

  public async Task<Result<GetUserDto>> Handle(GetActiveUserQuery request, CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var currentUserName = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    var criteria = UserQueryCriteria.FindUserName(currentUserName!);

    var user = await _userRepository.GetQueryAsync(criteria, cancellationToken: cancellationToken);
    return Result.Success(_mapper.Map<GetUserDto>(user[0]));
  }
}
