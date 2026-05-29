namespace AdventureWorksAIWorkspaceAPI.Domain.Reports;

public class Report
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string OriginalPrompt { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string? ResultJson { get; set; }

    public string? ChartsJson { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Draft;

    public bool IsFavorite { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ReportConversation? Conversation { get; set; }

    public ICollection<GeneratedSqlQuery> GeneratedSqlQueries { get; } = [];
}
