using AutoMapper;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.InvestmentDtos;

namespace FinanceApp.Application.Mappings;

public class InvestmentProfile : Profile
{
  public InvestmentProfile()
  {
    CreateMap<Domain.Entities.Investment, GetInvestmentDto>();
    CreateMap<UpdateInvestmentDto, Domain.Entities.Investment>();
    CreateMap<CreateInvestmentDto, Domain.Entities.Investment>();
  }
}
