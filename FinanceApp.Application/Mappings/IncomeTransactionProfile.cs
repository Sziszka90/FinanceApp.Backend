using AutoMapper;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;

namespace FinanceApp.Application.Mappings;

public class IncomeTransactionProfile : Profile
{
  public IncomeTransactionProfile()
  {
    CreateMap<Domain.Entities.IncomeTransaction, GetIncomeTransactionDto>();
    CreateMap<UpdateIncomeTransactionDto, Domain.Entities.IncomeTransaction>();
    CreateMap<CreateIncomeTransactionDto, Domain.Entities.IncomeTransaction>();
  }
}
