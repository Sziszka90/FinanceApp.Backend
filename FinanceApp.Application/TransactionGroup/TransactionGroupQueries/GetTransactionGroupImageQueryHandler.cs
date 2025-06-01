using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.TransactionGroup.TransactionGroupQueries;

public class GetTransactionGroupImageQueryHandler : IQueryHandler<GetTransactionGroupImageQuery, Result<Icon>>
{
  private readonly IMapper _mapper;
  private readonly ITransactionGroupRepository _transactionGroupRepository;

  public GetTransactionGroupImageQueryHandler(IMapper mapper, ITransactionGroupRepository transactionGroupRepository)
  {
    _mapper = mapper;
    _transactionGroupRepository = transactionGroupRepository;
  }

  public async Task<Result<Icon>> Handle(GetTransactionGroupImageQuery request, CancellationToken cancellationToken)
  {
    var result = await _transactionGroupRepository.GetByIdWithLimitAndIconAsync(request.Id, cancellationToken);
    return Result.Success(_mapper.Map<Icon>(result));
  }
}
