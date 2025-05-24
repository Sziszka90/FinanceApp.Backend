using AutoMapper;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.SavingDtos;

namespace FinanceApp.Application.Mappings;

public class SavingProfile : Profile
{
    public SavingProfile()
  {
    CreateMap<Domain.Entities.Saving, GetSavingDto>();
    CreateMap<UpdateSavingDto, Domain.Entities.Saving>();
    CreateMap<CreateSavingDto, Domain.Entities.Saving>();
  }
}
