using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.QueryCriteria;

public static class TransactionQueryCriteria
{
  /// <summary>
  /// Finds transactions with the same name as the one provided in the request.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>QueryCriteria<Transaction></returns>
  public static QueryCriteria<Transaction> FindDuplicatedName(CreateTransactionDto request)
  {
    var builder = new QueryCriteriaBuilder<Transaction>();

    builder.Where(x => x.Name == request.Name);

    return builder.Build();
  }

  /// <summary>
  /// Finds transactions with the same name as the one provided in the request.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>QueryCriteria<TransactionGroup></returns>
  public static QueryCriteria<TransactionGroup> FindDuplicatedName(CreateTransactionGroupDto request)
  {
    var builder = new QueryCriteriaBuilder<TransactionGroup>();

    builder.Where(x => x.Name == request.Name);

    return builder.Build();
  }

  /// <summary>
  /// Finds transactions with the same name as the one provided in the request.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>QueryCriteria<Transaction></returns>
  public static QueryCriteria<Transaction> FindDuplicatedNameExludingId(UpdateTransactionDto request)
  {
    var builder = new QueryCriteriaBuilder<Transaction>();

    builder.Where(x => x.Name == request.Name);
    builder.Where(x => x.Id != request.Id);

    return builder.Build();
  }

  /// <summary>
  /// Finds transaction groups with the same name as the one provided in the request, excluding the specified ID.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>QueryCriteria<TransactionGroup></returns>
  public static QueryCriteria<TransactionGroup> FindDuplicatedNameExludingId(UpdateTransactionGroupDto request)
  {
    var builder = new QueryCriteriaBuilder<TransactionGroup>();

    builder.Where(x => x.Name == request.Name);
    builder.Where(x => x.Id != request.Id);

    return builder.Build();
  }
}

public static class UserQueryCriteria
{
  /// <summary>
  /// Finds a user by username.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>QueryCriteria<User></returns>
  public static QueryCriteria<User> FindUserName(CreateUserDto request)
  {
    var builder = new QueryCriteriaBuilder<User>();

    builder.Where(x => x.UserName == request.UserName);

    return builder.Build();
  }

  /// <summary>
  /// Finds a user by username.
  /// </summary>
  /// <param name="userName"></param>
  /// <returns>QueryCriteria<User></returns>
  public static QueryCriteria<User> FindUserName(string userName)
  {
    var builder = new QueryCriteriaBuilder<User>();

    builder.Where(x => x.UserName == userName);

    return builder.Build();
  }

  /// <summary>
  /// Finds a user by email.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>QueryCriteria<User></returns>
  public static QueryCriteria<User> FindUserEmail(CreateUserDto request)
  {
    var builder = new QueryCriteriaBuilder<User>();

    builder.Where(x => x.Email == request.Email);

    return builder.Build();
  }

  /// <summary>
  /// Finds a user by email.
  /// </summary>
  /// <param name="email"></param>
  /// <returns>QueryCriteria<User></returns>
  public static QueryCriteria<User> FindUserEmail(string email)
  {
    var builder = new QueryCriteriaBuilder<User>();

    builder.Where(x => x.Email == email);

    return builder.Build();
  }
}
