using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionGroupApi.TransactionGroupQueries.GetTransactionGroupById;

public class GetTransactionGroupByIdQueryHandler : IQueryHandler<GetTransactionGroupByIdQuery, Result<GetTransactionGroupDto>>
{
  private readonly IMapper _mapper;
  private readonly ITransactionGroupRepository _transactionGroupRepository;

  public GetTransactionGroupByIdQueryHandler(IMapper mapper, ITransactionGroupRepository transactionGroupRepository)
  {
    _mapper = mapper;
    _transactionGroupRepository = transactionGroupRepository;
  }

  public async Task<Result<GetTransactionGroupDto>> Handle(GetTransactionGroupByIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _transactionGroupRepository.GetByIdAsync(request.Id, cancellationToken);
    return Result.Success(_mapper.Map<GetTransactionGroupDto>(result));
  }
}
