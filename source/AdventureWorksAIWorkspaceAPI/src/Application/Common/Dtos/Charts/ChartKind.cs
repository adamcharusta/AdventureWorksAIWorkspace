namespace AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Charts;

/// <summary>
/// The kind of visualization suggested for a report result. Values map to MUI X Charts component
/// types on the frontend.
/// </summary>
public enum ChartKind
{
    /// <summary>No meaningful chart; render the rows as a table.</summary>
    Table,

    /// <summary>Category comparison (MUI BarChart).</summary>
    Bar,

    /// <summary>Trend over an ordered/date axis (MUI LineChart).</summary>
    Line,

    /// <summary>Parts of a whole (MUI PieChart).</summary>
    Pie,

    /// <summary>Filled trend over an ordered axis (MUI LineChart with area).</summary>
    Area
}
