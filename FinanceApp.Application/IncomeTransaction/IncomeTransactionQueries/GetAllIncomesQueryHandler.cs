using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionQueries;

public class GetAllIncomesQueryHandler : IQueryHandler<GetAllIncomesQuery, Result<List<GetIncomeTransactionDto>>>
{
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.IncomeTransaction> _incomeTransactionRepository;

  public GetAllIncomesQueryHandler(IMapper mapper, IRepository<Domain.Entities.IncomeTransaction> incomeTransactionRepository)
  {
    _mapper = mapper;
    _incomeTransactionRepository = incomeTransactionRepository;
  }

  public async Task<Result<List<GetIncomeTransactionDto>>> Handle(GetAllIncomesQuery request, CancellationToken cancellationToken)
  {
    var result = await _incomeTransactionRepository.GetAllAsync(false, cancellationToken);
    return Result.Success(_mapper.Map<List<GetIncomeTransactionDto>>(result));
  }
}
