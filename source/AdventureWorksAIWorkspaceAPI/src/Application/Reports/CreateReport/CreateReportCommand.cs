using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports.CreateReport;

public sealed record CreateReportCommand(string Message, string? CurrentUserId = null);

public sealed record CreateReportRequest(string Message);

public static class CreateReportCommandHandler
{
    public static async Task<ReportChatResponse> Handle(
        CreateReportCommand command,
        IReportRepository reportRepository,
        IAiSqlGenerator sqlGenerator,
        ISqlSafetyValidator sqlValidator,
        IAdventureWorksQueryExecutor queryExecutor,
        IReportVisualizer reportVisualizer,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.CurrentUserId))
        {
            throw new UnauthorizedException("Authenticated user identifier is missing.");
        }

        var now = DateTime.UtcNow;
        var report = new Report
        {
            UserId = command.CurrentUserId,
            Title = ReportChatWorkflow.CreateTitle(command.Message),
            OriginalPrompt = command.Message,
            Status = ReportStatus.Generating,
            CreatedAt = now,
            UpdatedAt = now
        };

        var conversation = new ReportConversation
        {
            ReportId = report.Id,
            CreatedAt = now,
            UpdatedAt = now
        };
        report.Conversation = conversation;

        var userMessage = new ReportMessage
        {
            ConversationId = conversation.Id,
            Role = ReportMessageRole.User,
            Content = command.Message,
            SortOrder = 1,
            CreatedAt = now
        };
        conversation.Messages.Add(userMessage);

        ReportChatResponse response = await ReportChatWorkflow.ProcessAsync(
            report,
            userMessage,
            sqlGenerator,
            sqlValidator,
            queryExecutor,
            reportVisualizer,
            generateTitle: true,
            intentClassifier: null,
            cancellationToken);

        await reportRepository.AddAsync(report, cancellationToken);
        await reportRepository.SaveChangesAsync(cancellationToken);

        return response;
    }
}
