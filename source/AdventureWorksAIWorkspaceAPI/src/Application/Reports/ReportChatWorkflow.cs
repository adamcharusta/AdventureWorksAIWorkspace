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
    private const int MaxRecentMessages = 6;
    private const int MaxContextTextLength = 1200;
    private const int MaxSqlContextLength = 3000;

    /// <summary>
    /// Total number of SQL generation attempts, including the initial one. When validation or
    /// execution fails, the error is fed back to the model so it can self-correct.
    /// </summary>
    private const int MaxSqlAttempts = 3;

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
        IReportIntentClassifier? intentClassifier,
        CancellationToken cancellationToken)
    {
        ReportConversation conversation = report.Conversation
            ?? throw new InvalidOperationException("Report conversation is missing.");

        report.Status = ReportStatus.Generating;
        UpdateTimestamps(report, conversation, DateTime.UtcNow);

        // When the follow-up refines the most-recent section, overwrite it in place instead of
        // appending a new section. A null target means "add a new section" (the default).
        GeneratedSqlQuery? refineTarget =
            await ResolveRefineTargetAsync(report, conversation, userMessage, intentClassifier, cancellationToken);

        GeneratedSqlQuery? sqlQuery = null;
        TabularResult? result = null;
        IReadOnlyList<ChartSpec> charts = [];
        ReportOutcome outcome;
        string? message;
        string assistantContent;

        try
        {
            SqlAttemptFailure? previousFailure = null;

            for (int attempt = 1; ; attempt++)
            {
                bool isLastAttempt = attempt >= MaxSqlAttempts;

                AiSqlGenerationContext sqlContext =
                    BuildSqlGenerationContext(report, conversation, userMessage, previousFailure);
                GeneratedSql generated = await sqlGenerator.GenerateSqlAsync(
                    userMessage.Content,
                    sqlContext,
                    cancellationToken);

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

                    if (!isLastAttempt)
                    {
                        previousFailure = new SqlAttemptFailure(generated.Sql, validation.Reason!);
                        continue;
                    }

                    report.Status = ReportStatus.Failed;
                    report.Conclusions = null;
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
                }
                catch (QueryExecutionException exception)
                {
                    sqlQuery.ExecutionStatus = SqlExecutionStatus.Failed;
                    sqlQuery.ExecutionMessage = exception.Message;

                    if (!isLastAttempt)
                    {
                        previousFailure = new SqlAttemptFailure(generated.Sql, exception.Message);
                        continue;
                    }

                    report.Status = ReportStatus.Failed;
                    report.Summary = "The generated SQL passed validation but could not be executed.";
                    report.Conclusions = null;
                    report.ResultJson = null;
                    report.ChartsJson = null;
                    message = exception.Message;
                    outcome = ReportOutcome.ExecutionFailed;
                    assistantContent = $"The generated SQL could not be executed: {exception.Message}";

                    return CreateResponse(report, conversation, userMessage, assistantContent, sqlQuery, outcome, message, result, charts);
                }

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

                string resultJson = JsonSerializer.Serialize(result, JsonOptions);
                string chartsJson = JsonSerializer.Serialize(charts, JsonOptions);

                // Write the section content onto the refine target (overwrite in place, keeping its
                // CreatedAt so it stays in its slot) or onto this new query (append a new section).
                // The working query stays as an executed audit row; without section content it is
                // excluded from the rendered sections, so the refined result never duplicates.
                GeneratedSqlQuery section = refineTarget ?? sqlQuery;
                if (refineTarget is not null)
                {
                    refineTarget.SqlText = generated.Sql;
                    refineTarget.ValidationStatus = SqlValidationStatus.Valid;
                    refineTarget.ExecutionStatus = SqlExecutionStatus.Executed;
                    refineTarget.ResultRowCount = result.RowCount;
                    refineTarget.ResultColumnCount = result.Columns.Count;
                    refineTarget.DurationMs = result.ElapsedMilliseconds;
                }

                section.PresentationTitle = CreateSectionTitle(userMessage.Content, presentation.Title);
                section.Summary = presentation.Insights;
                section.Conclusions = presentation.Conclusions;
                section.ResultJson = resultJson;
                section.ChartsJson = chartsJson;

                report.Status = ReportStatus.Ready;
                report.Summary = presentation.Insights;
                report.Conclusions = presentation.Conclusions;
                report.ResultJson = resultJson;
                report.ChartsJson = chartsJson;
                message = null;
                outcome = ReportOutcome.Executed;
                assistantContent = presentation.Insights;

                return CreateResponse(report, conversation, userMessage, assistantContent, section, outcome, message, result, charts);
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

            return CreateResponse(report, conversation, userMessage, assistantContent, sqlQuery, outcome, message, result, charts);
        }
    }

    private static async Task<GeneratedSqlQuery?> ResolveRefineTargetAsync(
        Report report,
        ReportConversation conversation,
        ReportMessage userMessage,
        IReportIntentClassifier? intentClassifier,
        CancellationToken cancellationToken)
    {
        if (intentClassifier is null)
        {
            return null;
        }

        GeneratedSqlQuery? lastSection = report.GeneratedSqlQueries
            .Where(query => query.ExecutionStatus == SqlExecutionStatus.Executed)
            .Where(query => !string.IsNullOrWhiteSpace(query.Summary)
                            || !string.IsNullOrWhiteSpace(query.ResultJson)
                            || !string.IsNullOrWhiteSpace(query.ChartsJson))
            .OrderBy(query => query.CreatedAt)
            .LastOrDefault();

        if (lastSection is null)
        {
            return null;
        }

        IReadOnlyList<string> recentMessages = conversation.Messages
            .Where(message => message.Id != userMessage.Id)
            .OrderBy(message => message.SortOrder)
            .TakeLast(MaxRecentMessages)
            .Select(message => $"{message.Role}: {TrimForContext(message.Content, MaxContextTextLength)}")
            .ToArray();

        string? lastSectionTitle = string.IsNullOrWhiteSpace(lastSection.PresentationTitle)
            ? lastSection.UserPrompt
            : lastSection.PresentationTitle;

        ReportChatIntent intent = await intentClassifier.ClassifyAsync(
            new ReportIntentRequest(userMessage.Content, recentMessages, lastSectionTitle),
            cancellationToken);

        return intent == ReportChatIntent.RefineLastSection ? lastSection : null;
    }

    private static AiSqlGenerationContext BuildSqlGenerationContext(
        Report report,
        ReportConversation conversation,
        ReportMessage currentMessage,
        SqlAttemptFailure? previousFailure)
    {
        IReadOnlyList<string> recentMessages = conversation.Messages
            .Where(message => message.Id != currentMessage.Id)
            .OrderBy(message => message.SortOrder)
            .TakeLast(MaxRecentMessages)
            .Select(message => $"{message.Role}: {TrimForContext(message.Content, MaxContextTextLength)}")
            .ToArray();

        string? lastSuccessfulSql = report.GeneratedSqlQueries
            .Where(query => query.ExecutionStatus == SqlExecutionStatus.Executed)
            .OrderByDescending(query => query.CreatedAt)
            .Select(query => TrimForContext(query.SqlText, MaxSqlContextLength))
            .FirstOrDefault();

        return new AiSqlGenerationContext(
            TrimForContext(report.OriginalPrompt, MaxContextTextLength),
            TrimForContext(report.Summary, MaxContextTextLength),
            recentMessages,
            lastSuccessfulSql,
            previousFailure);
    }

    private static string? TrimForContext(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string text = value.Trim();
        return text.Length <= maxLength ? text : string.Concat(text.AsSpan(0, maxLength - 3), "...");
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

    public static string CreateSectionTitle(string userPrompt, string? presentationTitle)
    {
        string title = string.IsNullOrWhiteSpace(presentationTitle)
            ? userPrompt.Trim()
            : presentationTitle.Trim();

        return CreateTitle(title);
    }

    public static void UpdateTimestamps(Report report, ReportConversation conversation, DateTime timestamp)
    {
        report.UpdatedAt = timestamp;
        conversation.UpdatedAt = timestamp;
    }
}
