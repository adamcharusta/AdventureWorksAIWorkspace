using System.Text.Json;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Charts;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Sql;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.Reports.GenerateReport;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports;

internal static class ReportChatWorkflow
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<ReportChatResponse> ProcessAsync(
        Report report,
        ReportMessage userMessage,
        IAiSqlGenerator sqlGenerator,
        ISqlSafetyValidator sqlValidator,
        IAdventureWorksQueryExecutor queryExecutor,
        IReportVisualizer reportVisualizer,
        bool generateTitle,
        CancellationToken cancellationToken)
    {
        ReportConversation conversation = report.Conversation
            ?? throw new InvalidOperationException("Report conversation is missing.");

        report.Status = ReportStatus.Generating;
        UpdateTimestamps(report, conversation, DateTime.UtcNow);

        GeneratedSqlQuery? sqlQuery = null;
        TabularResult? result = null;
        IReadOnlyList<ChartSpec> charts = [];
        ReportOutcome outcome;
        string? message;
        string assistantContent;

        try
        {
            GeneratedSql generated = await sqlGenerator.GenerateSqlAsync(userMessage.Content, cancellationToken);

            sqlQuery = new GeneratedSqlQuery
            {
                ReportId = report.Id,
                SourceMessageId = userMessage.Id,
                UserPrompt = userMessage.Content,
                SqlText = generated.Sql,
                InputTokens = generated.InputTokens,
                OutputTokens = generated.OutputTokens,
                CreatedAt = DateTime.UtcNow
            };
            report.GeneratedSqlQueries.Add(sqlQuery);

            SqlValidationResult validation = sqlValidator.Validate(generated.Sql);
            if (!validation.IsValid)
            {
                sqlQuery.ValidationStatus = SqlValidationStatus.Rejected;
                sqlQuery.ValidationMessage = validation.Reason;
                sqlQuery.ExecutionStatus = SqlExecutionStatus.NotExecuted;

                report.Status = ReportStatus.Failed;
                report.ResultJson = null;
                report.ChartsJson = null;
                message = validation.Reason;
                outcome = ReportOutcome.Rejected;
                assistantContent = $"The generated SQL was rejected by safety validation: {validation.Reason}";

                return CreateResponse(report, conversation, userMessage, assistantContent, sqlQuery, outcome, message, result, charts);
            }

            sqlQuery.ValidationStatus = SqlValidationStatus.Valid;

            try
            {
                result = await queryExecutor.ExecuteQueryAsync(generated.Sql, cancellationToken);

                sqlQuery.ExecutionStatus = SqlExecutionStatus.Executed;
                sqlQuery.ResultRowCount = result.RowCount;
                sqlQuery.ResultColumnCount = result.Columns.Count;
                sqlQuery.DurationMs = result.ElapsedMilliseconds;

                ReportPresentation presentation = await reportVisualizer.CreatePresentationAsync(
                    userMessage.Content, result, cancellationToken);
                charts = presentation.Charts;

                if (generateTitle && !string.IsNullOrWhiteSpace(presentation.Title))
                {
                    report.Title = CreateTitle(presentation.Title);
                }

                report.Status = ReportStatus.Ready;
                report.Summary = presentation.Insights;
                report.ResultJson = JsonSerializer.Serialize(result, JsonOptions);
                report.ChartsJson = JsonSerializer.Serialize(charts, JsonOptions);
                message = null;
                outcome = ReportOutcome.Executed;
                assistantContent = presentation.Insights;
            }
            catch (QueryExecutionException exception)
            {
                sqlQuery.ExecutionStatus = SqlExecutionStatus.Failed;
                sqlQuery.ExecutionMessage = exception.Message;

                report.Status = ReportStatus.Failed;
                report.Summary = "The generated SQL passed validation but could not be executed.";
                report.ResultJson = null;
                report.ChartsJson = null;
                message = exception.Message;
                outcome = ReportOutcome.ExecutionFailed;
                assistantContent = $"The generated SQL could not be executed: {exception.Message}";
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            report.Status = ReportStatus.Failed;
            report.Summary = "The AI report generation request failed.";
            report.ResultJson = null;
            report.ChartsJson = null;
            message = exception.Message;
            outcome = ReportOutcome.GenerationFailed;
            assistantContent = $"The report could not be generated: {exception.Message}";
        }

        return CreateResponse(report, conversation, userMessage, assistantContent, sqlQuery, outcome, message, result, charts);
    }

    private static ReportChatResponse CreateResponse(
        Report report,
        ReportConversation conversation,
        ReportMessage userMessage,
        string assistantContent,
        GeneratedSqlQuery? sqlQuery,
        ReportOutcome outcome,
        string? message,
        TabularResult? result,
        IReadOnlyList<ChartSpec> charts)
    {
        var now = DateTime.UtcNow;
        var assistantMessage = new ReportMessage
        {
            ConversationId = conversation.Id,
            Role = ReportMessageRole.Assistant,
            Content = assistantContent,
            SortOrder = GetNextSortOrder(conversation),
            RelatedSqlQueryId = sqlQuery?.Id,
            CreatedAt = now
        };

        conversation.Messages.Add(assistantMessage);
        UpdateTimestamps(report, conversation, now);

        return new ReportChatResponse(
            ReportMapping.ToDetailsDto(report),
            ReportMapping.ToMessageDto(userMessage),
            ReportMapping.ToMessageDto(assistantMessage),
            sqlQuery is null ? null : ReportMapping.ToSqlQueryDto(sqlQuery),
            outcome,
            message,
            result,
            charts);
    }

    public static int GetNextSortOrder(ReportConversation conversation) =>
        conversation.Messages.Count == 0 ? 1 : conversation.Messages.Max(message => message.SortOrder) + 1;

    public static string CreateTitle(string message)
    {
        string title = message.Trim();

        if (title.Length <= 80)
        {
            return title;
        }

        return string.Concat(title.AsSpan(0, 77), "...");
    }

    public static void UpdateTimestamps(Report report, ReportConversation conversation, DateTime timestamp)
    {
        report.UpdatedAt = timestamp;
        conversation.UpdatedAt = timestamp;
    }
}
