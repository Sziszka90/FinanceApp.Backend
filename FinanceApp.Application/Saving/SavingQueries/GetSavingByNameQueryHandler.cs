using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Saving.SavingQueries;

public class GetSavingByNameQueryHandler : IQueryHandler<GetSavingByIdQuery, Result<GetSavingDto>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.Saving> _savingRepository;

  #endregion

  #region Constructors

  public GetSavingByNameQueryHandler(IMapper mapper, IRepository<Domain.Entities.Saving> savingRepository)
  {
    _mapper = mapper;
    _savingRepository = savingRepository;
  }

  #endregion

  #region Methods

  public async Task<Result<GetSavingDto>> Handle(GetSavingByIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _savingRepository.GetByIdAsync(request.Id, cancellationToken);
    return Result.Success(_mapper.Map<GetSavingDto>(result));
  }

  #endregion
}
