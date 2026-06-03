using AdventureWorksAIWorkspace.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspace.Application.Common.Dtos.Charts;
using AdventureWorksAIWorkspace.Domain.Reports;

namespace AdventureWorksAIWorkspace.Application.Reports;

public sealed record ReportSummaryDto(
    string Id,
    string Title,
    ReportStatus Status,
    bool IsFavorite,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ReportMessageDto(
    string Id,
    ReportMessageRole Role,
    string Content,
    int SortOrder,
    string? RelatedSqlQueryId,
    DateTime CreatedAt);

public sealed record GeneratedSqlQueryDto(
    string Id,
    string? SourceMessageId,
    string UserPrompt,
    string SqlText,
    string? Explanation,
    SqlValidationStatus ValidationStatus,
    string? ValidationMessage,
    SqlExecutionStatus ExecutionStatus,
    string? ExecutionMessage,
    int? InputTokens,
    int? OutputTokens,
    int? ResultRowCount,
    int? ResultColumnCount,
    long? DurationMs,
    DateTime CreatedAt);

public sealed record ReportSectionDto(
    string Id,
    string? SourceMessageId,
    string Question,
    string Title,
    string Insights,
    string? Conclusions,
    TabularResult? Result,
    IReadOnlyList<ChartSpec> Charts,
    DateTime CreatedAt);

public sealed record ReportDetailsDto(
    string Id,
    string Title,
    string OriginalPrompt,
    string? Summary,
    string? Conclusions,
    ReportStatus Status,
    bool IsFavorite,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    TabularResult? Result,
    IReadOnlyList<ChartSpec> Charts,
    IReadOnlyList<ReportSectionDto> Sections,
    IReadOnlyList<ReportMessageDto> Messages,
    IReadOnlyList<GeneratedSqlQueryDto> GeneratedSqlQueries);

public sealed record ReportChatResponse(
    ReportDetailsDto Report,
    ReportMessageDto UserMessage,
    ReportMessageDto AssistantMessage,
    GeneratedSqlQueryDto? SqlQuery,
    ReportOutcome Outcome,
    string? Message,
    TabularResult? Result,
    IReadOnlyList<ChartSpec> Charts);
