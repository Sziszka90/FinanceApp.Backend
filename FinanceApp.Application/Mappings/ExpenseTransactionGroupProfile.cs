using AutoMapper;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Mappings;

public class ExpenseTransactionGroupProfile : Profile
{
  public ExpenseTransactionGroupProfile()
  {
    CreateMap<Domain.Entities.ExpenseTransactionGroup, GetExpenseTransactionGroupDto>();

    CreateMap<UpdateExpenseTransactionGroupDto, Domain.Entities.ExpenseTransactionGroup>();

    CreateMap<CreateExpenseTransactionGroupDto, Domain.Entities.ExpenseTransactionGroup>();

    CreateMap<Money, Money>();
  }
}
