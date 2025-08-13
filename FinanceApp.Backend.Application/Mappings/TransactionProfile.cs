using AutoMapper;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.Mappings;

public class TransactionProfile : Profile
{
  public TransactionProfile()
  {
    CreateMap<UpdateTransactionDto, Transaction>()
      .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => src.TransactionDate.ToUniversalTime()));

    CreateMap<CreateTransactionDto, Transaction>()
      .ForMember(dest => dest.TransactionDate, opt => opt.MapFrom(src => src.TransactionDate.ToUniversalTime()));
  }
}
