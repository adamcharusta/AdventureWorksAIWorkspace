using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports.GenerateReport;

/// <summary>
/// The result of an AI-driven report generation request.
/// </summary>
/// <param name="Question">The original natural-language question.</param>
/// <param name="Sql">The SQL produced by the model. Returned for every outcome, for transparency.</param>
/// <param name="Outcome">Whether the SQL was executed, rejected by validation, or failed at execution.</param>
/// <param name="Message">Why the SQL was not executed for a non-<see cref="ReportOutcome.Executed"/> outcome; otherwise null.</param>
/// <param name="Result">The query result when executed; otherwise null.</param>
/// <param name="InputTokens">Prompt tokens consumed by the model, when reported.</param>
/// <param name="OutputTokens">Completion tokens produced by the model, when reported.</param>
public sealed record GenerateReportResponse(
    string Question,
    string Sql,
    ReportOutcome Outcome,
    string? Message,
    TabularResult? Result,
    int? InputTokens,
    int? OutputTokens);
