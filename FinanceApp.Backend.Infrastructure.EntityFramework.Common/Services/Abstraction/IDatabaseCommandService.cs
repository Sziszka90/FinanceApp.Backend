using System.Data.Common;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services.Abstraction;

public interface IDatabaseCommandService
{
  /// <summary>
  /// Executes a SQL query and maps the results to a list of type T.
  /// </summary>
  /// <param name="sql">The SQL query to execute.</param>
  /// <param name="parameters">The parameters to include in the query.</param>
  /// <param name="mapper">A function to map the results to type T.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>A list of mapped results.</returns>
  Task<List<T>> ExecuteQueryAsync<T>(
      string sql,
      Dictionary<string, object> parameters,
      Func<DbDataReader, T> mapper,
      CancellationToken cancellationToken = default);

  /// <summary>
  /// Executes a SQL query and returns a single scalar value.
  /// </summary>
  /// <param name="sql">The SQL query to execute.</param>
  /// <param name="parameters">The parameters to include in the query.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>The scalar value returned by the query.</returns>
  Task<T?> ExecuteScalarAsync<T>(
      string sql,
      Dictionary<string, object> parameters,
      CancellationToken cancellationToken = default);

  /// <summary>
  /// Executes a SQL query and returns the number of affected rows.
  /// </summary>
  /// <param name="sql">The SQL query to execute.</param>
  /// <param name="parameters">The parameters to include in the query.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>The number of affected rows.</returns>
  Task<int> ExecuteNonQueryAsync(
      string sql,
      Dictionary<string, object> parameters,
      CancellationToken cancellationToken = default);
}
