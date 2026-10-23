using System.Data;
using System.Data.Common;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services.Abstraction;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services;

public class DatabaseCommandService : IDatabaseCommandService
{
  private readonly FinanceAppDbContext _dbContext;

  public DatabaseCommandService(FinanceAppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<List<T>> ExecuteQueryAsync<T>(
      string sql,
      Dictionary<string, object> parameters,
      Func<DbDataReader, T> mapper,
      CancellationToken cancellationToken = default)
  {
    using var command = await CreateCommandAsync(sql, parameters);
    var result = new List<T>();

    using var reader = await command.ExecuteReaderAsync(cancellationToken);
    while (await reader.ReadAsync(cancellationToken))
    {
      result.Add(mapper(reader));
    }

    return result;
  }

  public async Task<T?> ExecuteScalarAsync<T>(
      string sql,
      Dictionary<string, object> parameters,
      CancellationToken cancellationToken = default)
  {
    using var command = await CreateCommandAsync(sql, parameters);
    var result = await command.ExecuteScalarAsync(cancellationToken);

    if (result == null || result == DBNull.Value)
    {
      return default(T);
    }

    try
    {
      return (T)Convert.ChangeType(result, typeof(T));
    }
    catch
    {
      return result is T typedResult ? typedResult : default(T);
    }
  }

  public async Task<int> ExecuteNonQueryAsync(
      string sql,
      Dictionary<string, object> parameters,
      CancellationToken cancellationToken = default)
  {
    using var command = await CreateCommandAsync(sql, parameters);
    return await command.ExecuteNonQueryAsync(cancellationToken);
  }

  private async Task<DbCommand> CreateCommandAsync(string sql, Dictionary<string, object> parameters)
  {
    var connection = _dbContext.Database.GetDbConnection();

    if (connection.State != ConnectionState.Open)
    {
      await connection.OpenAsync();
    }

    var command = connection.CreateCommand();
    command.CommandText = sql;


    foreach (var parameter in parameters)
    {
      var dbParam = command.CreateParameter();
      dbParam.ParameterName = parameter.Key;
      dbParam.Value = parameter.Value ?? DBNull.Value;
      command.Parameters.Add(dbParam);
    }

    return command;
  }
}
