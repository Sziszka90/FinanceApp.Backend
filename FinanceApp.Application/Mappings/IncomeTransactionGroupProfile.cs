using AutoMapper;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;

namespace FinanceApp.Application.Mappings;

public class IncomeTransactionGroupProfile : Profile
{
  #region Constructors

  public IncomeTransactionGroupProfile()
  {
    CreateMap<Domain.Entities.IncomeTransactionGroup, GetIncomeTransactionGroupDto>();
    CreateMap<UpdateIncomeTransactionGroupDto, Domain.Entities.IncomeTransactionGroup>();
    CreateMap<CreateIncomeTransactionGroupDto, Domain.Entities.IncomeTransactionGroup>();
  }

  #endregion
}