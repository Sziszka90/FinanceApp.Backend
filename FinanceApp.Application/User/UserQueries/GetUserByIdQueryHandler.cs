using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.User.UserQueries;

namespace FinanceApp.User.UserQueries.SavingQueries;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, Result<GetUserDto>>
{
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.User> _userRepository;

  public GetUserByIdQueryHandler(IMapper mapper, IRepository<Domain.Entities.User> userRepository)
  {
    _mapper = mapper;
    _userRepository = userRepository;
  }

  public async Task<Result<GetUserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
    return Result.Success(_mapper.Map<GetUserDto>(user));
  }
}
