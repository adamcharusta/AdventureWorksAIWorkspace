using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;

namespace AdventureWorksAIWorkspaceAPI.Application.Common.Services;

/// <summary>
/// Turns a natural-language business question into read-only SQL for the AdventureWorks database.
/// </summary>
/// <remarks>
/// Implementations build the model prompt and call <see cref="IAiChatClient"/>. The returned SQL
/// is untrusted and must be passed through <see cref="ISqlSafetyValidator"/> before execution.
/// </remarks>
public interface IAiSqlGenerator
{
    /// <summary>
    /// Generates a read-only SQL statement that answers the supplied question.
    /// </summary>
    /// <param name="question">The user's natural-language business question.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The generated SQL together with token usage when available.</returns>
    Task<GeneratedSql> GenerateSqlAsync(string question, CancellationToken cancellationToken = default);
}
