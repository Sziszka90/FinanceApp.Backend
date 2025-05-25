using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionGroup.TransactionGroupQueries;

public class GetAllTransactionGroupQueryHandler : IQueryHandler<GetAllTransactionGroupsQuery, Result<List<GetTransactionGroupDto>>>
{
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.TransactionGroup> _transactionGroupRepository;

  public GetAllTransactionGroupQueryHandler(IMapper mapper, IRepository<Domain.Entities.TransactionGroup> transactionGroupRepository)
  {
    _mapper = mapper;
    _transactionGroupRepository = transactionGroupRepository;
  }

  public async Task<Result<List<GetTransactionGroupDto>>> Handle(GetAllTransactionGroupsQuery request, CancellationToken cancellationToken)
  {
    var result = await _transactionGroupRepository.GetAllAsync(false, cancellationToken);
    return Result.Success(_mapper.Map<List<GetTransactionGroupDto>>(result));
  }
}
