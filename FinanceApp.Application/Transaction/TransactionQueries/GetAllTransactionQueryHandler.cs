using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Transaction.TransactionQueries;

public class GetAllTransactionQueryHandler : IQueryHandler<GetAllTransactionQuery, Result<List<GetTransactionDto>>>
{
  private readonly IMapper _mapper;
  private readonly ITransactionRepository _transactionRepository;

  public GetAllTransactionQueryHandler(IMapper mapper, ITransactionRepository transactionRepository)
  {
    _mapper = mapper;
    _transactionRepository = transactionRepository;
  }

  public async Task<Result<List<GetTransactionDto>>> Handle(GetAllTransactionQuery request, CancellationToken cancellationToken)
  {
    var result = await _transactionRepository.GetAllAsync(request.TransactionFilter, false, cancellationToken);
    return Result.Success(_mapper.Map<List<GetTransactionDto>>(result));
  }
}
