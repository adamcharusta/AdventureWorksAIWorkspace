using System.Text.Json;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Charts;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Sql;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;
using Ardalis.GuardClauses;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports;

/// <summary>
/// Default <see cref="IReportChatPipeline"/>. Orchestrates the AI report turn: build SQL context,
/// generate SQL, validate, execute, and present, retrying with model feedback on failure.
/// </summary>
public sealed class ReportChatPipeline : IReportChatPipeline
{
    private const int MaxRecentMessages = 6;
    private const int MaxContextTextLength = 1200;
    private const int MaxSqlContextLength = 3000;

    /// <summary>
    /// Total number of SQL generation attempts, including the initial one. When validation or
    /// execution fails, the error is fed back to the model so it can self-correct.
    /// </summary>
    private const int MaxSqlAttempts = 3;

    private readonly IAiSqlGenerator _sqlGenerator;
    private readonly ISqlSafetyValidator _sqlValidator;
    private readonly IAdventureWorksQueryExecutor _queryExecutor;
    private readonly IReportVisualizer _reportVisualizer;
    private readonly IReportIntentClassifier _intentClassifier;

    public ReportChatPipeline(
        IAiSqlGenerator sqlGenerator,
        ISqlSafetyValidator sqlValidator,
        IAdventureWorksQueryExecutor queryExecutor,
        IReportVisualizer reportVisualizer,
        IReportIntentClassifier intentClassifier)
    {
        Guard.Against.Null(sqlGenerator);
        Guard.Against.Null(sqlValidator);
        Guard.Against.Null(queryExecutor);
        Guard.Against.Null(reportVisualizer);
        Guard.Against.Null(intentClassifier);

        _sqlGenerator = sqlGenerator;
        _sqlValidator = sqlValidator;
        _queryExecutor = queryExecutor;
        _reportVisualizer = reportVisualizer;
        _intentClassifier = intentClassifier;
    }

    public async Task<ReportChatResponse> ProcessAsync(
        Report report,
        ReportMessage userMessage,
        bool generateTitle,
        bool classifyIntent,
        CancellationToken cancellationToken)
    {
        ReportConversation conversation = report.Conversation
            ?? throw new InvalidOperationException("Report conversation is missing.");

        report.Status = ReportStatus.Generating;
        ReportChatWorkflow.UpdateTimestamps(report, conversation, DateTime.UtcNow);

        // When the follow-up refines the most-recent section, overwrite it in place instead of
        // appending a new section. A null target means "add a new section" (the default).
        GeneratedSqlQuery? refineTarget = classifyIntent
            ? await ResolveRefineTargetAsync(report, conversation, userMessage, cancellationToken)
            : null;

        return await RunAttemptsAsync(report, conversation, userMessage, refineTarget, generateTitle, cancellationToken);
    }

    private async Task<ReportChatResponse> RunAttemptsAsync(
        Report report,
        ReportConversation conversation,
        ReportMessage userMessage,
        GeneratedSqlQuery? refineTarget,
        bool generateTitle,
        CancellationToken cancellationToken)
    {
        GeneratedSqlQuery? workingQuery = null;

        try
        {
            SqlAttemptFailure? previousFailure = null;

            for (int attempt = 1; ; attempt++)
            {
                bool isLastAttempt = attempt >= MaxSqlAttempts;

                AiSqlGenerationContext sqlContext =
                    BuildSqlGenerationContext(report, conversation, userMessage, previousFailure);
                GeneratedSql generated = await _sqlGenerator.GenerateSqlAsync(
                    userMessage.Content, sqlContext, cancellationToken);

                workingQuery = AppendWorkingQuery(report, userMessage, generated);

                SqlValidationResult validation = _sqlValidator.Validate(generated.Sql);
                if (!validation.IsValid)
                {
                    workingQuery.ValidationStatus = SqlValidationStatus.Rejected;
                    workingQuery.ValidationMessage = validation.Reason;
                    workingQuery.ExecutionStatus = SqlExecutionStatus.NotExecuted;

                    if (!isLastAttempt)
                    {
                        previousFailure = new SqlAttemptFailure(generated.Sql, validation.Reason!);
                        continue;
                    }

                    return FailRejected(report, conversation, userMessage, workingQuery, validation.Reason);
                }

                workingQuery.ValidationStatus = SqlValidationStatus.Valid;

                TabularResult result;
                try
                {
                    result = await _queryExecutor.ExecuteQueryAsync(generated.Sql, cancellationToken);
                }
                catch (QueryExecutionException exception)
                {
                    workingQuery.ExecutionStatus = SqlExecutionStatus.Failed;
                    workingQuery.ExecutionMessage = exception.Message;

                    if (!isLastAttempt)
                    {
                        previousFailure = new SqlAttemptFailure(generated.Sql, exception.Message);
                        continue;
                    }

                    return FailExecution(report, conversation, userMessage, workingQuery, exception.Message);
                }

                return await SucceedAsync(
                    report, conversation, userMessage, workingQuery, refineTarget, result, generated, generateTitle,
                    cancellationToken);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return FailGeneration(report, conversation, userMessage, workingQuery, exception);
        }
    }

    private async Task<ReportChatResponse> SucceedAsync(
        Report report,
        ReportConversation conversation,
        ReportMessage userMessage,
        GeneratedSqlQuery workingQuery,
        GeneratedSqlQuery? refineTarget,
        TabularResult result,
        GeneratedSql generated,
        bool generateTitle,
        CancellationToken cancellationToken)
    {
        workingQuery.ExecutionStatus = SqlExecutionStatus.Executed;
        workingQuery.ResultRowCount = result.RowCount;
        workingQuery.ResultColumnCount = result.Columns.Count;
        workingQuery.DurationMs = result.ElapsedMilliseconds;

        ReportPresentation presentation = await _reportVisualizer.CreatePresentationAsync(
            userMessage.Content, result, cancellationToken);
        IReadOnlyList<ChartSpec> charts = presentation.Charts;

        if (generateTitle && !string.IsNullOrWhiteSpace(presentation.Title))
        {
            report.Title = ReportChatWorkflow.CreateTitle(presentation.Title);
        }

        string resultJson = JsonSerializer.Serialize(result, ReportJson.Options);
        string chartsJson = JsonSerializer.Serialize(charts, ReportJson.Options);

        // Write the section content onto the refine target (overwrite in place, keeping its
        // CreatedAt so it stays in its slot) or onto this new query (append a new section).
        // The working query stays as an executed audit row; without section content it is
        // excluded from the rendered sections, so the refined result never duplicates.
        GeneratedSqlQuery section = refineTarget ?? workingQuery;
        if (refineTarget is not null)
        {
            refineTarget.SqlText = generated.Sql;
            refineTarget.ValidationStatus = SqlValidationStatus.Valid;
            refineTarget.ExecutionStatus = SqlExecutionStatus.Executed;
            refineTarget.ResultRowCount = result.RowCount;
            refineTarget.ResultColumnCount = result.Columns.Count;
            refineTarget.DurationMs = result.ElapsedMilliseconds;
        }

        section.PresentationTitle = ReportChatWorkflow.CreateSectionTitle(userMessage.Content, presentation.Title);
        section.Summary = presentation.Insights;
        section.Conclusions = presentation.Conclusions;
        section.ResultJson = resultJson;
        section.ChartsJson = chartsJson;

        report.Status = ReportStatus.Ready;
        report.Summary = presentation.Insights;
        report.Conclusions = presentation.Conclusions;
        report.ResultJson = resultJson;
        report.ChartsJson = chartsJson;

        return CreateResponse(
            report, conversation, userMessage, presentation.Insights, section,
            ReportOutcome.Executed, message: null, result, charts);
    }

    private ReportChatResponse FailRejected(
        Report report,
        ReportConversation conversation,
        ReportMessage userMessage,
        GeneratedSqlQuery sqlQuery,
        string? reason)
    {
        report.Status = ReportStatus.Failed;
        report.Conclusions = null;
        report.ResultJson = null;
        report.ChartsJson = null;

        return CreateResponse(
            report, conversation, userMessage,
            $"The generated SQL was rejected by safety validation: {reason}",
            sqlQuery, ReportOutcome.Rejected, reason, result: null, charts: []);
    }

    private ReportChatResponse FailExecution(
        Report report,
        ReportConversation conversation,
        ReportMessage userMessage,
        GeneratedSqlQuery sqlQuery,
        string message)
    {
        report.Status = ReportStatus.Failed;
        report.Summary = "The generated SQL passed validation but could not be executed.";
        report.Conclusions = null;
        report.ResultJson = null;
        report.ChartsJson = null;

        return CreateResponse(
            report, conversation, userMessage,
            $"The generated SQL could not be executed: {message}",
            sqlQuery, ReportOutcome.ExecutionFailed, message, result: null, charts: []);
    }

    private ReportChatResponse FailGeneration(
        Report report,
        ReportConversation conversation,
        ReportMessage userMessage,
        GeneratedSqlQuery? sqlQuery,
        Exception exception)
    {
        report.Status = ReportStatus.Failed;
        report.Summary = "The AI report generation request failed.";
        report.ResultJson = null;
        report.ChartsJson = null;

        return CreateResponse(
            report, conversation, userMessage,
            $"The report could not be generated: {exception.Message}",
            sqlQuery, ReportOutcome.GenerationFailed, exception.Message, result: null, charts: []);
    }

    private static GeneratedSqlQuery AppendWorkingQuery(
        Report report, ReportMessage userMessage, GeneratedSql generated)
    {
        var sqlQuery = new GeneratedSqlQuery
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
        return sqlQuery;
    }

    private async Task<GeneratedSqlQuery?> ResolveRefineTargetAsync(
        Report report,
        ReportConversation conversation,
        ReportMessage userMessage,
        CancellationToken cancellationToken)
    {
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

        ReportChatIntent intent = await _intentClassifier.ClassifyAsync(
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
            SortOrder = ReportChatWorkflow.GetNextSortOrder(conversation),
            RelatedSqlQueryId = sqlQuery?.Id,
            CreatedAt = now
        };

        conversation.Messages.Add(assistantMessage);
        ReportChatWorkflow.UpdateTimestamps(report, conversation, now);

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
}
