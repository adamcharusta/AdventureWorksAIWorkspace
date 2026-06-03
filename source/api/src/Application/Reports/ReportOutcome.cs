namespace AdventureWorksAIWorkspace.Application.Reports;

/// <summary>
/// The outcome of an AI-driven report generation request.
/// </summary>
public enum ReportOutcome
{
    /// <summary>The SQL passed validation and was executed successfully.</summary>
    Executed,

    /// <summary>The generated SQL was rejected by safety validation and was not executed.</summary>
    Rejected,

    /// <summary>The SQL passed validation but failed while executing against AdventureWorks.</summary>
    ExecutionFailed,

    /// <summary>The AI request failed before SQL could be generated.</summary>
    GenerationFailed
}
