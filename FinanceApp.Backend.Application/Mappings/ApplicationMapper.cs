using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace FinanceApp.Backend.Application.Mappings;

public interface IApplicationMapper
{
  GetTransactionDto ToGetTransactionDto(Transaction source);
  List<GetTransactionDto> ToGetTransactionDtos(IEnumerable<Transaction> source);
  GetTransactionGroupDto ToGetTransactionGroupDto(TransactionGroup source);
  List<GetTransactionGroupDto> ToGetTransactionGroupDtos(IEnumerable<TransactionGroup> source);
  GetUserDto ToGetUserDto(User source);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ApplicationMapper : IApplicationMapper
{
  public partial GetTransactionDto ToGetTransactionDto(Transaction source);
  public partial List<GetTransactionDto> ToGetTransactionDtos(IEnumerable<Transaction> source);
  public partial GetTransactionGroupDto ToGetTransactionGroupDto(TransactionGroup source);
  public partial List<GetTransactionGroupDto> ToGetTransactionGroupDtos(IEnumerable<TransactionGroup> source);
  public partial GetUserDto ToGetUserDto(User source);
}
