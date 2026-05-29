namespace AdventureWorksAIWorkspaceAPI.Domain.Reports;

public class ReportConversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ReportId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Report? Report { get; set; }

    public ICollection<ReportMessage> Messages { get; } = [];
}
