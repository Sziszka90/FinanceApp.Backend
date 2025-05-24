using AutoMapper;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.ExpenseTransactionDtos;

namespace FinanceApp.Application.Mappings;

public class ExpenseTransactionProfile : Profile
{
  public ExpenseTransactionProfile()
  {
    CreateMap<Domain.Entities.ExpenseTransaction, GetExpenseTransactionDto>();
    CreateMap<UpdateExpenseTransactionDto, Domain.Entities.ExpenseTransaction>();
    CreateMap<CreateExpenseTransactionDto, Domain.Entities.ExpenseTransaction>();
  }
}
