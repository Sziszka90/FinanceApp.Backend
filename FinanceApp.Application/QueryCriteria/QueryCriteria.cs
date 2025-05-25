using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.InvestmentDtos;
using FinanceApp.Application.Dtos.SavingDtos;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.QueryCriteria;

public static class TransactionQueryCriteria
{
  public static QueryCriteria<Domain.Entities.Transaction> FindDuplicatedName(CreateTransactionDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.Transaction>();

    builder.Where(x => x.Name == request.Name);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.TransactionGroup> FindDuplicatedName(CreateTransactionGroupDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.TransactionGroup>();

    builder.Where(x => x.Name == request.Name);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.Transaction> FindDuplicatedNameExludingId(UpdateTransactionDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.Transaction>();

    builder.Where(x => x.Name == request.Name);
    builder.Where(x => x.Id != request.Id);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.TransactionGroup> FindDuplicatedNameExludingId(UpdateTransactionGroupDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.TransactionGroup>();

    builder.Where(x => x.Name == request.Name);
    builder.Where(x => x.Id != request.Id);

    return builder.Build();
  }
}

public static class InvestmentQueryCriteria
{
  public static QueryCriteria<Domain.Entities.Investment> FindDuplicatedName(CreateInvestmentDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.Investment>();

    builder.Where(x => x.Name == request.Name);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.Investment> FindDuplicatedNameExludingId(UpdateInvestmentDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.Investment>();

    builder.Where(x => x.Name == request.Name);
    builder.Where(x => x.Id != request.Id);

    return builder.Build();
  }
}

public static class SavingQueryCriteria
{
  public static QueryCriteria<Domain.Entities.Saving> FindDuplicatedName(CreateSavingDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.Saving>();

    builder.Where(x => x.Name == request.Name);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.Saving> FindDuplicatedNameExludingId(UpdateSavingDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.Saving>();

    builder.Where(x => x.Name == request.Name);
    builder.Where(x => x.Id != request.Id);

    return builder.Build();
  }
}

public static class UserQueryCriteria
{
  public static QueryCriteria<Domain.Entities.User> FindUserName(CreateUserDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.User>();

    builder.Where(x => x.UserName == request.UserName);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.User> FindUserName(string userName)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.User>();

    builder.Where(x => x.UserName == userName);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.User> FindDuplicatedUserNameExludingId(UpdateUserDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.User>();

    builder.Where(x => x.UserName == request.UserName);
    builder.Where(x => x.Id != request.Id);

    return builder.Build();
  }
}
