using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.UserApi.UserQueries.GetUserById;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, Result<GetUserDto>>
{
  private readonly ILogger<GetUserByIdQueryHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.User> _userRepository;

  public GetUserByIdQueryHandler(
    ILogger<GetUserByIdQueryHandler> logger,
    IMapper mapper,
    IRepository<Domain.Entities.User> userRepository)
  {
    _logger = logger;
    _mapper = mapper;
    _userRepository = userRepository;
  }

  public async Task<Result<GetUserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(request.Id, noTracking: true, cancellationToken: cancellationToken);
    _logger.LogDebug("Retrieved user with ID:{Id}", request.Id);
    return Result.Success(_mapper.Map<GetUserDto>(user));
  }
}
