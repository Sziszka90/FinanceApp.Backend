using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Transaction.TransactionQueries;

public class GetTransactionByIdQueryHandler : IQueryHandler<GetTransactionByIdQuery, Result<GetTransactionDto>>
{
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.Transaction> _transactionRepository;

  public GetTransactionByIdQueryHandler(IMapper mapper, IRepository<Domain.Entities.Transaction> transactionRepository)
  {
    _mapper = mapper;
    _transactionRepository = transactionRepository;
  }

  public async Task<Result<GetTransactionDto>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);
    return Result.Success(_mapper.Map<GetTransactionDto>(result));
  }
}
