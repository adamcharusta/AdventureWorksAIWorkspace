namespace AdventureWorksAIWorkspace.Application.Common.Dtos.AdventureWorks;

/// <summary>
/// Generic, shape-agnostic result of a read-only AdventureWorks query.
/// </summary>
/// <remarks>
/// The AdventureWorks analytical database is queried with dynamic, AI-generated SQL, so the
/// result shape is not known at compile time. This contract carries the column metadata, the
/// row values, and execution metadata needed to render dashboards without modelling the schema.
/// </remarks>
/// <param name="Columns">Ordered column metadata for the result set.</param>
/// <param name="Rows">Row values, aligned to <paramref name="Columns"/> by index. Null cells represent SQL NULL.</param>
/// <param name="RowCount">The number of rows returned in <paramref name="Rows"/>.</param>
/// <param name="Truncated">True when the result was cut off because it reached the configured row limit.</param>
/// <param name="ElapsedMilliseconds">Time spent reading the result set, in milliseconds.</param>
public sealed record TabularResult(
    IReadOnlyList<TabularColumn> Columns,
    IReadOnlyList<IReadOnlyList<object?>> Rows,
    int RowCount,
    bool Truncated,
    long ElapsedMilliseconds);
