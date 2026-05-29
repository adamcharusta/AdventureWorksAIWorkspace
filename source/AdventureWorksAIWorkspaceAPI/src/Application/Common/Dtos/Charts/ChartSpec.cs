namespace AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Charts;

/// <summary>
/// A frontend-agnostic chart specification produced for a report result. It references columns of
/// the accompanying tabular result by name; the frontend joins this specification with the result
/// rows to render the chart (for example with MUI X Charts).
/// </summary>
/// <param name="Kind">The chart type to render.</param>
/// <param name="Title">A short chart title.</param>
/// <param name="CategoryColumn">The column used for the x-axis (Bar/Line/Area) or labels (Pie); null for Table.</param>
/// <param name="Series">The numeric value columns to plot. Empty for Table.</param>
/// <param name="Description">An optional one-line description of what the chart shows.</param>
public sealed record ChartSpec(
    ChartKind Kind,
    string Title,
    string? CategoryColumn,
    IReadOnlyList<ChartSeries> Series,
    string? Description);
