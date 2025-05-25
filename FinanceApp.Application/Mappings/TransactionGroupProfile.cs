using AutoMapper;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.TransactionGroupDtos;

namespace FinanceApp.Application.Mappings;

public class TransactionGroupProfile : Profile
{
  public TransactionGroupProfile()
  {
    CreateMap<Domain.Entities.TransactionGroup, GetTransactionGroupDto>();
    CreateMap<UpdateTransactionGroupDto, Domain.Entities.TransactionGroup>();
    CreateMap<CreateTransactionGroupDto, Domain.Entities.TransactionGroup>();
  }
}
