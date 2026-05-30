using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports.AddReportMessage;

public sealed record AddReportMessageCommand(string ReportId, string Message, string? CurrentUserId = null);

public sealed record AddReportMessageRequest(string Message);

public static class AddReportMessageCommandHandler
{
    public static async Task<ReportChatResponse> Handle(
        AddReportMessageCommand command,
        IReportRepository reportRepository,
        IAiSqlGenerator sqlGenerator,
        ISqlSafetyValidator sqlValidator,
        IAdventureWorksQueryExecutor queryExecutor,
        IReportVisualizer reportVisualizer,
        IReportIntentClassifier intentClassifier,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.CurrentUserId))
        {
            throw new UnauthorizedException("Authenticated user identifier is missing.");
        }

        Report report = await reportRepository.GetOwnedReportAsync(
            command.ReportId,
            command.CurrentUserId,
            cancellationToken)
            ?? throw new NotFoundException($"Report with ID '{command.ReportId}' was not found.");

        ReportConversation conversation = report.Conversation
            ?? throw new InvalidOperationException("Report conversation is missing.");

        var now = DateTime.UtcNow;
        var userMessage = new ReportMessage
        {
            ConversationId = conversation.Id,
            Role = ReportMessageRole.User,
            Content = command.Message,
            SortOrder = ReportChatWorkflow.GetNextSortOrder(conversation),
            CreatedAt = now
        };
        conversation.Messages.Add(userMessage);
        ReportChatWorkflow.UpdateTimestamps(report, conversation, now);

        ReportChatResponse response = await ReportChatWorkflow.ProcessAsync(
            report,
            userMessage,
            sqlGenerator,
            sqlValidator,
            queryExecutor,
            reportVisualizer,
            generateTitle: false,
            intentClassifier,
            cancellationToken);

        await reportRepository.SaveChangesAsync(cancellationToken);

        return response;
    }
}
