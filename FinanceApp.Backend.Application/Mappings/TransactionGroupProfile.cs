using AutoMapper;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;

namespace FinanceApp.Backend.Application.Mappings;

public class TransactionGroupProfile : Profile
{
  public TransactionGroupProfile()
  {
    CreateMap<Domain.Entities.TransactionGroup, GetTransactionGroupDto>();
    CreateMap<UpdateTransactionGroupDto, Domain.Entities.TransactionGroup>();
    CreateMap<CreateTransactionGroupDto, Domain.Entities.TransactionGroup>();
    CreateMap<Domain.Entities.TransactionGroup, TopTransactionGroupDto>();
  }
}
