using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Investment.InvestmentQueries;

public class GetAllInvestmentsQueryHandler : IQueryHandler<GetAllInvestmentsQuery, Result<List<GetInvestmentDto>>>
{
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.Investment> _investmentRepository;

  public GetAllInvestmentsQueryHandler(IMapper mapper, IRepository<Domain.Entities.Investment> investmentRepository)
  {
    _mapper = mapper;
    _investmentRepository = investmentRepository;
  }

  public async Task<Result<List<GetInvestmentDto>>> Handle(GetAllInvestmentsQuery request, CancellationToken cancellationToken)
  {
    var result = await _investmentRepository.GetAllAsync(false, cancellationToken);
    return Result.Success(_mapper.Map<List<GetInvestmentDto>>(result));
  }
}
