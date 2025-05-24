using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionQueries;

public class GetIncomeByNameQueryHandler : IQueryHandler<GetIncomeByIdQuery, Result<GetIncomeTransactionDto>>
{
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.IncomeTransaction> _incomeTransactionRepository;

  public GetIncomeByNameQueryHandler(IMapper mapper, IRepository<Domain.Entities.IncomeTransaction> incomeTransactionRepository)
  {
    _mapper = mapper;
    _incomeTransactionRepository = incomeTransactionRepository;
  }

  public async Task<Result<GetIncomeTransactionDto>> Handle(GetIncomeByIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _incomeTransactionRepository.GetByIdAsync(request.Id, cancellationToken);
    return Result.Success(_mapper.Map<GetIncomeTransactionDto>(result));
  }
}
