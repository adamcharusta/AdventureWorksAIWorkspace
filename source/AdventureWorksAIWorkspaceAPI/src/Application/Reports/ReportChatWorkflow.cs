using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports;

/// <summary>
/// Pure helpers shared by the report chat handlers and <see cref="ReportChatPipeline"/> for sort
/// ordering, title shaping, and timestamp bookkeeping.
/// </summary>
internal static class ReportChatWorkflow
{
    private const int MaxTitleLength = 80;

    public static int GetNextSortOrder(ReportConversation conversation) =>
        conversation.Messages.Count == 0 ? 1 : conversation.Messages.Max(message => message.SortOrder) + 1;

    public static string CreateTitle(string message)
    {
        string title = message.Trim();

        if (title.Length <= MaxTitleLength)
        {
            return title;
        }

        return string.Concat(title.AsSpan(0, MaxTitleLength - 3), "...");
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
