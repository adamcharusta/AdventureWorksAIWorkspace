namespace AdventureWorksAIWorkspace.Application.Common.Dtos.Charts;

/// <summary>
/// A single data series in a chart, mapped to a numeric column of the report result.
/// </summary>
/// <param name="Column">The result column name that supplies the series values.</param>
/// <param name="Label">An optional human-friendly label; falls back to the column name when null.</param>
public sealed record ChartSeries(string Column, string? Label);
