using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransactionGroup.IncomeTransactionGroupQueries;

public class GetAllIncomeGroupQueryHandler : IQueryHandler<GetAllIncomeGroupsQuery, Result<List<GetIncomeTransactionGroupDto>>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.IncomeTransactionGroup> _incomeTransactionGroupRepository;

  #endregion

  #region Constructors

  public GetAllIncomeGroupQueryHandler(IMapper mapper, IRepository<Domain.Entities.IncomeTransactionGroup> incomeTransactionGroupRepository)
  {
    _mapper = mapper;
    _incomeTransactionGroupRepository = incomeTransactionGroupRepository;
  }

  #endregion

  #region Methods

  public async Task<Result<List<GetIncomeTransactionGroupDto>>> Handle(GetAllIncomeGroupsQuery request, CancellationToken cancellationToken)
  {
    var result = await _incomeTransactionGroupRepository.GetAllAsync(false, cancellationToken);
    return Result.Success(_mapper.Map<List<GetIncomeTransactionGroupDto>>(result));
  }

  #endregion
}