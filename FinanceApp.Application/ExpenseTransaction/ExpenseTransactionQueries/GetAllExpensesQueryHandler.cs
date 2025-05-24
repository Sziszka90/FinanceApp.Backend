using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.ExpenseTransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.ExpenseTransaction.ExpenseTransactionQueries;

public class GetAllExpensesQueryHandler : IQueryHandler<GetAllExpensesQuery, Result<List<GetExpenseTransactionDto>>>
{
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.ExpenseTransaction> _expenseTransactionRepository;

  public GetAllExpensesQueryHandler(IMapper mapper, IRepository<Domain.Entities.ExpenseTransaction> expenseTransactionRepository)
  {
    _mapper = mapper;
    _expenseTransactionRepository = expenseTransactionRepository;
  }

  public async Task<Result<List<GetExpenseTransactionDto>>> Handle(GetAllExpensesQuery request, CancellationToken cancellationToken)
  {
    var result = await _expenseTransactionRepository.GetAllAsync(false, cancellationToken);
    return Result.Success(_mapper.Map<List<GetExpenseTransactionDto>>(result));
  }
}
