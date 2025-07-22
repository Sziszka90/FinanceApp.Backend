using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetTransactionGroupById;

public class GetTransactionGroupByIdQueryHandler : IQueryHandler<GetTransactionGroupByIdQuery, Result<GetTransactionGroupDto>>
{
  private readonly IMapper _mapper;
  private readonly ITransactionGroupRepository _transactionGroupRepository;

  public GetTransactionGroupByIdQueryHandler(
    IMapper mapper,
    ITransactionGroupRepository transactionGroupRepository)
  {
    _mapper = mapper;
    _transactionGroupRepository = transactionGroupRepository;
  }

  public async Task<Result<GetTransactionGroupDto>> Handle(GetTransactionGroupByIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _transactionGroupRepository.GetByIdAsync(request.Id, noTracking: true, cancellationToken: cancellationToken);
    return Result.Success(_mapper.Map<GetTransactionGroupDto>(result));
  }
}
