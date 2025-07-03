using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.QueryCriteria;

public static class TransactionQueryCriteria
{
  /// <summary>
  /// Finds transactions with the same name as the one provided in the request.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>QueryCriteria<Domain.Entities.Transaction></returns>
  public static QueryCriteria<Domain.Entities.Transaction> FindDuplicatedName(CreateTransactionDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.Transaction>();

    builder.Where(x => x.Name == request.Name);

    return builder.Build();
  }

  /// <summary>
  /// Finds transactions with the same name as the one provided in the request.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>QueryCriteria<Domain.Entities.TransactionGroup></returns>
  public static QueryCriteria<Domain.Entities.TransactionGroup> FindDuplicatedName(CreateTransactionGroupDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.TransactionGroup>();

    builder.Where(x => x.Name == request.Name);

    return builder.Build();
  }

  /// <summary>
  /// Finds transactions with the same name as the one provided in the request.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>QueryCriteria<Domain.Entities.Transaction></returns>
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

public static class UserQueryCriteria
{
  /// <summary>
  /// Finds a user by username.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>QueryCriteria<Domain.Entities.User></returns>
  public static QueryCriteria<Domain.Entities.User> FindUserName(CreateUserDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.User>();

    builder.Where(x => x.UserName == request.UserName);

    return builder.Build();
  }

  /// <summary>
  /// Finds a user by username.
  /// </summary>
  /// <param name="userName"></param>
  /// <returns>QueryCriteria<Domain.Entities.User></returns>
  public static QueryCriteria<Domain.Entities.User> FindUserName(string userName)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.User>();

    builder.Where(x => x.UserName == userName);

    return builder.Build();
  }

  /// <summary>
  /// Finds a user by email.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>QueryCriteria<Domain.Entities.User></returns>
  public static QueryCriteria<Domain.Entities.User> FindUserEmail(CreateUserDto request)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.User>();

    builder.Where(x => x.Email == request.Email);

    return builder.Build();
  }

  /// <summary>
  /// Finds a user by email.
  /// </summary>
  /// <param name="email"></param>
  /// <returns>QueryCriteria<Domain.Entities.User></returns>
  public static QueryCriteria<Domain.Entities.User> FindUserEmail(string email)
  {
    var builder = new QueryCriteriaBuilder<Domain.Entities.User>();

    builder.Where(x => x.Email == email);

    return builder.Build();
  }
}
