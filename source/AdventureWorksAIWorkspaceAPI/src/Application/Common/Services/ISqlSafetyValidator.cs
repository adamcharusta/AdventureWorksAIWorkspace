using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Sql;

namespace AdventureWorksAIWorkspaceAPI.Application.Common.Services;

/// <summary>
/// Validates AI-generated SQL before it is executed against the AdventureWorks analytical
/// database.
/// </summary>
/// <remarks>
/// Model output is untrusted. Every statement must pass this validator before it reaches
/// <see cref="IAdventureWorksQueryExecutor"/>. The MVP implementation applies rule-based checks
/// (read-only entry point, single statement, blocked destructive keywords); parser-based
/// validation and table/column allowlists are future enhancements.
/// </remarks>
public interface ISqlSafetyValidator
{
    /// <summary>
    /// Checks whether the supplied SQL is safe to execute as a read-only query.
    /// </summary>
    /// <param name="sql">The SQL statement to validate.</param>
    /// <returns>A result indicating whether the statement is allowed, with a reason when rejected.</returns>
    SqlValidationResult Validate(string sql);
}
