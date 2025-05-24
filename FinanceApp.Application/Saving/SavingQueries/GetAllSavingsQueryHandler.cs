using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Saving.SavingQueries;

public class GetAllSavingsQueryHandler : IQueryHandler<GetAllSavingsQuery, Result<List<GetSavingDto>>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.Saving> _savingRepository;

  #endregion

  #region Constructors

  public GetAllSavingsQueryHandler(IMapper mapper, IRepository<Domain.Entities.Saving> savingRepository)
  {
    _mapper = mapper;
    _savingRepository = savingRepository;
  }

  #endregion

  #region Methods

  public async Task<Result<List<GetSavingDto>>> Handle(GetAllSavingsQuery request, CancellationToken cancellationToken)
  {
    var result = await _savingRepository.GetAllAsync(false, cancellationToken);
    return Result.Success(_mapper.Map<List<GetSavingDto>>(result));
  }

  #endregion
}
