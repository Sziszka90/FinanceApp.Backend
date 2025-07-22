using AutoMapper;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;

namespace FinanceApp.Backend.Application.Mappings;

public class TransactionProfile : Profile
{
  public TransactionProfile()
  {
    CreateMap<Domain.Entities.Transaction, GetTransactionDto>();
    CreateMap<UpdateTransactionDto, Domain.Entities.Transaction>();
    CreateMap<CreateTransactionDto, Domain.Entities.Transaction>();
  }
}
