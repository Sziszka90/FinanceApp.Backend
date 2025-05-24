using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Investment.InvestmentQueries;

public class GetInvestmentByNameQueryHandler : IQueryHandler<GetInvestmentByIdQuery, Result<GetInvestmentDto>>
{
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.Investment> _investmentRepository;

  public GetInvestmentByNameQueryHandler(IMapper mapper, IRepository<Domain.Entities.Investment> investmentRepository)
  {
    _mapper = mapper;
    _investmentRepository = investmentRepository;
  }

  public async Task<Result<GetInvestmentDto>> Handle(GetInvestmentByIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _investmentRepository.GetByIdAsync(request.Id, cancellationToken);
    return Result.Success(_mapper.Map<GetInvestmentDto>(result));
  }
}
