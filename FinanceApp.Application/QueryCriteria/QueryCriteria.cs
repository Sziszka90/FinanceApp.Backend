using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.Dtos.InvestmentDtos;
using FinanceApp.Application.Dtos.SavingDtos;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.QueryCriteria;

public static class ExpenseQueryCriteria
{
  #region Methods

  public static QueryCriteria<Domain.Entities.ExpenseTransaction> FindDuplicatedName(CreateExpenseTransactionDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.ExpenseTransaction>();

    builder.Where(x => x.Name == request.Name);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.ExpenseTransactionGroup> FindDuplicatedName(CreateExpenseTransactionGroupDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.ExpenseTransactionGroup>();

    builder.Where(x => x.Name == request.Name);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.ExpenseTransaction> FindDuplicatedNameExludingId(UpdateExpenseTransactionDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.ExpenseTransaction>();

    builder.Where(x => x.Name == request.Name);
    builder.Where(x => x.Id != request.Id);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.ExpenseTransactionGroup> FindDuplicatedNameExludingId(UpdateExpenseTransactionGroupDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.ExpenseTransactionGroup>();

    builder.Where(x => x.Name == request.Name);
    builder.Where(x => x.Id != request.Id);

    return builder.Build();
  }

  #endregion
}

public static class IncomeQueryCriteria
{
  #region Methods

  public static QueryCriteria<Domain.Entities.IncomeTransaction> FindDuplicatedName(CreateIncomeTransactionDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.IncomeTransaction>();

    builder.Where(x => x.Name == request.Name);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.IncomeTransactionGroup> FindDuplicatedName(CreateIncomeTransactionGroupDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.IncomeTransactionGroup>();

    builder.Where(x => x.Name == request.Name);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.IncomeTransaction> FindDuplicatedNameExludingId(UpdateIncomeTransactionDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.IncomeTransaction>();

    builder.Where(x => x.Name == request.Name);
    builder.Where(x => x.Id != request.Id);

    return builder.Build();
  }

  public static QueryCriteria<Domain.Entities.IncomeTransactionGroup> FindDuplicatedNameExludingId(UpdateIncomeTransactionGroupDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.IncomeTransactionGroup>();

    builder.Where(x => x.Name == request.Name);
    builder.Where(x => x.Id != request.Id);

    return builder.Build();
  }

  #endregion
}

public static class InvestmentQueryCriteria
{
  #region Methods

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

  #endregion
}

public static class SavingQueryCriteria
{
  #region Methods

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

  #endregion
}

public static class UserQueryCriteria
{
  #region Methods

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

  #endregion
}