using AdventureWorksAIWorkspaceAPI.Application.Reports;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Common.Services;

/// <summary>
/// Runs the AI report turn for a single user message: generates SQL (retrying on validation or
/// execution failure), executes it against AdventureWorks, builds the presentation, and records the
/// outcome on the report. The caller owns creating the report/conversation and persisting changes.
/// </summary>
public interface IReportChatPipeline
{
    /// <summary>
    /// Processes <paramref name="userMessage"/> against <paramref name="report"/>, mutating the
    /// report, its conversation, and its generated-SQL history in place.
    /// </summary>
    /// <param name="report">The report to update; its <see cref="Report.Conversation"/> must be loaded.</param>
    /// <param name="userMessage">The user message that has already been appended to the conversation.</param>
    /// <param name="generateTitle">When true, the report title is set from the AI presentation title.</param>
    /// <param name="classifyIntent">
    /// When true, the message is classified to decide whether it refines the most recent section
    /// (overwrite in place) or adds a new one. Use false for the first message of a new report.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    Task<ReportChatResponse> ProcessAsync(
        Report report,
        ReportMessage userMessage,
        bool generateTitle,
        bool classifyIntent,
        CancellationToken cancellationToken);
}
