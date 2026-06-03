namespace AdventureWorksAIWorkspace.Application.Common.Dtos.Charts;

/// <summary>
/// The AI-produced presentation for a report result: business insights plus suggested charts.
/// </summary>
/// <param name="Insights">Plain-text business insights written by the model.</param>
/// <param name="Charts">Suggested chart specifications; may be empty when the data is not chartable.</param>
/// <param name="Title">A short, model-suggested report title; null when the model did not provide one.</param>
/// <param name="Conclusions">
/// Optional, free-text conclusions: deeper analysis, takeaways, trend interpretation, or
/// recommendations the model adds only when it judges they add value beyond the summary.
/// Null when the model had nothing useful to add.
/// </param>
public sealed record ReportPresentation(
    string Insights,
    IReadOnlyList<ChartSpec> Charts,
    string? Title = null,
    string? Conclusions = null);
