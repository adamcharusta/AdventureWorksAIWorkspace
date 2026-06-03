using System.Text.Json;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Charts;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports;

internal static class ReportMapping
{
    public static ReportSummaryDto ToSummaryDto(Report report) =>
        new(
            report.Id,
            report.Title,
            report.Status,
            report.IsFavorite,
            report.CreatedAt,
            report.UpdatedAt);

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

    public static ReportMessageDto ToMessageDto(ReportMessage message) =>
        new(
            message.Id,
            message.Role,
            message.Content,
            message.SortOrder,
            message.RelatedSqlQueryId,
            message.CreatedAt);

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
