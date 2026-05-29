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

        return new ReportDetailsDto(
            report.Id,
            report.Title,
            report.OriginalPrompt,
            report.Summary,
            report.Status,
            report.IsFavorite,
            report.CreatedAt,
            report.UpdatedAt,
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
}
