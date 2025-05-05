using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.ExpenseTransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.ExpenseTransaction.ExpenseTransactionQueries;

public class GetExpenseByIdQueryHandler : IQueryHandler<GetExpenseByIdQuery, Result<GetExpenseTransactionDto>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.ExpenseTransaction> _expenseTransactionRepository;

  #endregion

  #region Constructors

  public GetExpenseByIdQueryHandler(IMapper mapper, IRepository<Domain.Entities.ExpenseTransaction> expenseTransactionRepository)
  {
    _mapper = mapper;
    _expenseTransactionRepository = expenseTransactionRepository;
  }

  #endregion

  #region Methods

  public async Task<Result<GetExpenseTransactionDto>> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _expenseTransactionRepository.GetByIdAsync(request.Id, cancellationToken);
    return Result.Success(_mapper.Map<GetExpenseTransactionDto>(result));
  }

  #endregion
}