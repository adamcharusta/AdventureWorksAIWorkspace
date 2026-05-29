namespace AdventureWorksAIWorkspaceAPI.Domain.Reports;

public class ReportMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ConversationId { get; set; } = string.Empty;

    public ReportMessageRole Role { get; set; }

    public string Content { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string? RelatedSqlQueryId { get; set; }

    public DateTime CreatedAt { get; set; }

    public ReportConversation? Conversation { get; set; }

    public GeneratedSqlQuery? RelatedSqlQuery { get; set; }
}
