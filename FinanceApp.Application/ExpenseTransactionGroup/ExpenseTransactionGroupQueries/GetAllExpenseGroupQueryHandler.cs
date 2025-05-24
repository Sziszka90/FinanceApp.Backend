using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupQueries;

public class GetAllExpenseGroupQueryHandler : IQueryHandler<GetAllExpenseGroupsQuery, Result<List<GetExpenseTransactionGroupDto>>>
{
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.ExpenseTransactionGroup> _expenseTransactionGroupRepository;

  public GetAllExpenseGroupQueryHandler(IMapper mapper, IRepository<Domain.Entities.ExpenseTransactionGroup> expenseTransactionGroupRepository)
  {
    _mapper = mapper;
    _expenseTransactionGroupRepository = expenseTransactionGroupRepository;
  }

  public async Task<Result<List<GetExpenseTransactionGroupDto>>> Handle(GetAllExpenseGroupsQuery request, CancellationToken cancellationToken)
  {
    var result = await _expenseTransactionGroupRepository.GetAllAsync(false, cancellationToken);
    return Result.Success(_mapper.Map<List<GetExpenseTransactionGroupDto>>(result));
  }
}
