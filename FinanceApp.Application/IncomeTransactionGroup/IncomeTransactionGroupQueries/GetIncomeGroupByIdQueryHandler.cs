using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransactionGroup.IncomeTransactionGroupQueries;

public class GetIncomeGroupByIdQueryHandler : IQueryHandler<GetIncomeGroupByIdQuery, Result<GetIncomeTransactionGroupDto>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.IncomeTransactionGroup> _incomeTransactionGroupRepository;

  #endregion

  #region Constructors

  public GetIncomeGroupByIdQueryHandler(IMapper mapper, IRepository<Domain.Entities.IncomeTransactionGroup> incomeTransactionGroupRepository)
  {
    _mapper = mapper;
    _incomeTransactionGroupRepository = incomeTransactionGroupRepository;
  }

  #endregion

  #region Methods

  public async Task<Result<GetIncomeTransactionGroupDto>> Handle(GetIncomeGroupByIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _incomeTransactionGroupRepository.GetByIdAsync(request.Id, cancellationToken);
    return Result.Success(_mapper.Map<GetIncomeTransactionGroupDto>(result));
  }

  #endregion
}
