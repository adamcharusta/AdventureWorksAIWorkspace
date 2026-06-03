using AdventureWorksAIWorkspace.Application.Common.Dtos.AdventureWorks;

namespace AdventureWorksAIWorkspace.Application.Common.Services.AdventureWorks;

/// <summary>
/// Executes read-only SQL against the AdventureWorks analytical database and returns a generic
/// tabular result.
/// </summary>
/// <remarks>
/// Implementations must connect with read-only credentials and enforce a command timeout and a
/// result-size limit. SQL passed to this contract is expected to have already passed safety
/// validation; this abstraction does not validate the statement itself.
/// </remarks>
public interface IAdventureWorksQueryExecutor
{
    /// <summary>
    /// Executes the supplied read-only SQL and returns the result as columns and rows.
    /// </summary>
    /// <param name="sql">The validated, read-only SQL statement to execute.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The query result as generic columns, rows, and execution metadata.</returns>
    Task<TabularResult> ExecuteQueryAsync(string sql, CancellationToken cancellationToken = default);
}
