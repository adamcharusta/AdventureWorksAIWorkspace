namespace AdventureWorksAIWorkspace.Domain.Reports;

public class GeneratedSqlQuery
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ReportId { get; set; } = string.Empty;

    public string? SourceMessageId { get; set; }

    public string UserPrompt { get; set; } = string.Empty;

    public string SqlText { get; set; } = string.Empty;

    public string? Explanation { get; set; }

    public string? PresentationTitle { get; set; }

    public string? Summary { get; set; }

    public string? Conclusions { get; set; }

    public string? ResultJson { get; set; }

    public string? ChartsJson { get; set; }

    public SqlValidationStatus ValidationStatus { get; set; } = SqlValidationStatus.NotValidated;

    public string? ValidationMessage { get; set; }

    public SqlExecutionStatus ExecutionStatus { get; set; } = SqlExecutionStatus.NotExecuted;

    public string? ExecutionMessage { get; set; }

    public int? InputTokens { get; set; }

    public int? OutputTokens { get; set; }

    public int? ResultRowCount { get; set; }

    public int? ResultColumnCount { get; set; }

    public long? DurationMs { get; set; }

    public DateTime CreatedAt { get; set; }

    public Report? Report { get; set; }

    public ReportMessage? SourceMessage { get; set; }
}
