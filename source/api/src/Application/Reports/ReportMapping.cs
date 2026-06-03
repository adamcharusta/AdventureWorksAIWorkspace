using System.Text.Json;
using AdventureWorksAIWorkspace.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspace.Application.Common.Dtos.Charts;
using AdventureWorksAIWorkspace.Domain.Reports;

namespace AdventureWorksAIWorkspace.Application.Reports;

/// <summary>
/// Converts the mutable report aggregate into API DTOs. This is also where persisted JSON snapshots
/// are hydrated back into typed result/chart contracts for the frontend.
/// </summary>
internal static class ReportMapping
{
    /// <summary>
    /// Lightweight shape used by the report sidebar; does not require loading the conversation or
    /// generated SQL history.
    /// </summary>
    public static ReportSummaryDto ToSummaryDto(Report report) =>
        new(
            report.Id,
            report.Title,
            report.Status,
            report.IsFavorite,
            report.CreatedAt,
            report.UpdatedAt);

    /// <summary>
    /// Full report shape used by the workspace. Requires the report aggregate with conversation
    /// messages and generated SQL queries already loaded by the repository.
    /// </summary>
    public static ReportDetailsDto ToDetailsDto(Report report)
    {
        IReadOnlyList<ReportMessageDto> messages = report.Conversation?.Messages
            .OrderBy(message => message.SortOrder)
            .Select(ToMessageDto)
            .ToList() ?? [];

        IReadOnlyList<GeneratedSqlQueryDto> sqlQueries = report.GeneratedSqlQueries
            .OrderBy(query => query.CreatedAt)
            .Select(ToSqlQueryDto)
            .ToList();

        IReadOnlyList<ReportSectionDto> sections = CreateSections(report);

        return new ReportDetailsDto(
            report.Id,
            report.Title,
            report.OriginalPrompt,
            report.Summary,
            report.Conclusions,
            report.Status,
            report.IsFavorite,
            report.CreatedAt,
            report.UpdatedAt,
            Deserialize<TabularResult>(report.ResultJson),
            Deserialize<IReadOnlyList<ChartSpec>>(report.ChartsJson) ?? [],
            sections,
            messages,
            sqlQueries);
    }

    /// <summary>
    /// Maps one persisted conversation message into the wire contract while preserving sort order
    /// and optional linkage to the SQL query generated from that message.
    /// </summary>
    public static ReportMessageDto ToMessageDto(ReportMessage message) =>
        new(
            message.Id,
            message.Role,
            message.Content,
            message.SortOrder,
            message.RelatedSqlQueryId,
            message.CreatedAt);

    /// <summary>
    /// Maps SQL audit metadata. Result and chart payloads stay on report sections rather than this
    /// DTO so the client can render the conversation history and dashboard independently.
    /// </summary>
    public static GeneratedSqlQueryDto ToSqlQueryDto(GeneratedSqlQuery query) =>
        new(
            query.Id,
            query.SourceMessageId,
            query.UserPrompt,
            query.SqlText,
            query.Explanation,
            query.ValidationStatus,
            query.ValidationMessage,
            query.ExecutionStatus,
            query.ExecutionMessage,
            query.InputTokens,
            query.OutputTokens,
            query.ResultRowCount,
            query.ResultColumnCount,
            query.DurationMs,
            query.CreatedAt);

    /// <summary>
    /// Builds renderable dashboard sections from per-query snapshots.
    ///
    /// Newer reports store each successful turn on <see cref="GeneratedSqlQuery"/>. Older reports
    /// may only have the report-level latest snapshot, so this method falls back to a single section
    /// built from the report itself.
    /// </summary>
    private static IReadOnlyList<ReportSectionDto> CreateSections(Report report)
    {
        List<ReportSectionDto> sections = report.GeneratedSqlQueries
            .Where(query => query.ExecutionStatus == SqlExecutionStatus.Executed)
            .Where(query => !string.IsNullOrWhiteSpace(query.Summary)
                            || !string.IsNullOrWhiteSpace(query.ResultJson)
                            || !string.IsNullOrWhiteSpace(query.ChartsJson))
            .OrderBy(query => query.CreatedAt)
            .Select(ToSectionDto)
            .ToList();

        if (sections.Count > 0)
        {
            return sections;
        }

        TabularResult? result = Deserialize<TabularResult>(report.ResultJson);
        IReadOnlyList<ChartSpec> charts = Deserialize<IReadOnlyList<ChartSpec>>(report.ChartsJson) ?? [];
        if (result is null && string.IsNullOrWhiteSpace(report.Summary))
        {
            return [];
        }

        return
        [
            new ReportSectionDto(
                report.Id,
                SourceMessageId: null,
                report.OriginalPrompt,
                report.Title,
                report.Summary ?? "The report was generated successfully.",
                string.IsNullOrWhiteSpace(report.Conclusions) ? null : report.Conclusions.Trim(),
                result,
                charts,
                report.UpdatedAt)
        ];
    }

    /// <summary>
    /// Converts one successful SQL turn into a dashboard section. The query must already contain
    /// presentation JSON produced by the report visualization step.
    /// </summary>
    private static ReportSectionDto ToSectionDto(GeneratedSqlQuery query)
    {
        IReadOnlyList<ChartSpec> charts = Deserialize<IReadOnlyList<ChartSpec>>(query.ChartsJson) ?? [];

        return new ReportSectionDto(
            query.Id,
            query.SourceMessageId,
            query.UserPrompt,
            string.IsNullOrWhiteSpace(query.PresentationTitle)
                ? query.UserPrompt
                : query.PresentationTitle.Trim(),
            string.IsNullOrWhiteSpace(query.Summary)
                ? "The report section was generated successfully."
                : query.Summary.Trim(),
            string.IsNullOrWhiteSpace(query.Conclusions) ? null : query.Conclusions.Trim(),
            Deserialize<TabularResult>(query.ResultJson),
            charts,
            query.CreatedAt);
    }

    /// <summary>
    /// Best-effort deserialization for persisted snapshots. A malformed snapshot should not make
    /// the report unreadable; callers treat <c>null</c> as "no renderable payload".
    /// </summary>
    private static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, ReportJson.Options);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
