namespace AdventureWorksAIWorkspace.Application.Common.Dtos.Ai;

/// <summary>
/// Conversation and report state that helps the AI generate follow-up SQL for an existing report.
/// </summary>
/// <param name="OriginalPrompt">The first user prompt that created the report.</param>
/// <param name="CurrentSummary">The latest assistant summary stored on the report.</param>
/// <param name="RecentMessages">Recent conversation turns before the current user message.</param>
/// <param name="LastSuccessfulSql">The latest SQL statement that executed successfully.</param>
/// <param name="PreviousFailure">
/// The previous SQL attempt that failed validation or execution, when the model is being asked to
/// correct itself. <see langword="null"/> on the first attempt.
/// </param>
public sealed record AiSqlGenerationContext(
    string? OriginalPrompt,
    string? CurrentSummary,
    IReadOnlyList<string> RecentMessages,
    string? LastSuccessfulSql,
    SqlAttemptFailure? PreviousFailure = null);

/// <summary>
/// A previous SQL generation attempt that failed, used to prompt the model for a corrected query.
/// </summary>
/// <param name="FailedSql">The SQL produced by the previous attempt.</param>
/// <param name="Error">The validation or execution error returned for that SQL.</param>
public sealed record SqlAttemptFailure(string FailedSql, string Error);
