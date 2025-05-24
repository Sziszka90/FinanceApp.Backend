using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupQueries;

public class GetExpenseGroupByIdQueryHandler : IQueryHandler<GetExpenseGroupByIdQuery, Result<GetExpenseTransactionGroupDto>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.ExpenseTransactionGroup> _expenseTransactionGroupRepository;

  #endregion

  #region Constructors

  public GetExpenseGroupByIdQueryHandler(IMapper mapper, IRepository<Domain.Entities.ExpenseTransactionGroup> expenseTransactionGroupRepository)
  {
    _mapper = mapper;
    _expenseTransactionGroupRepository = expenseTransactionGroupRepository;
  }

  #endregion

  #region Methods

  public async Task<Result<GetExpenseTransactionGroupDto>> Handle(GetExpenseGroupByIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _expenseTransactionGroupRepository.GetByIdAsync(request.Id, cancellationToken);
    return Result.Success(_mapper.Map<GetExpenseTransactionGroupDto>(result));
  }

  #endregion
}
