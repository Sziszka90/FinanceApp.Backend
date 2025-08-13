using AutoMapper;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.Mappings;

public class TransactionProfile : Profile
{
  public TransactionProfile()
  {
    CreateMap<UpdateTransactionDto, Transaction>();
    CreateMap<CreateTransactionDto, Transaction>();
    CreateMap<Transaction, GetTransactionDto>();
  }
}
