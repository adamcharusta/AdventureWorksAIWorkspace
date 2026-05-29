using System.Data;
using System.Diagnostics;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using Ardalis.GuardClauses;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Services;

/// <summary>
/// Maps an open <see cref="IDataReader"/> into a generic <see cref="TabularResult"/>.
/// </summary>
/// <remarks>
/// Mapping is kept separate from connection and Dapper concerns so it can be tested without a
/// live database, for example with a <see cref="DataTableReader"/>.
/// </remarks>
public static class TabularResultReader
{
    /// <summary>
    /// Reads all columns and up to <paramref name="maxRows"/> rows from the reader.
    /// </summary>
    /// <param name="reader">An open reader positioned before the first row.</param>
    /// <param name="maxRows">Maximum number of rows to materialize before truncating.</param>
    /// <returns>The materialized tabular result.</returns>
    public static TabularResult Read(IDataReader reader, int maxRows)
    {
        Guard.Against.Null(reader);
        Guard.Against.NegativeOrZero(maxRows);

        var stopwatch = Stopwatch.StartNew();

        var columns = new TabularColumn[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
        {
            columns[i] = new TabularColumn(reader.GetName(i), reader.GetDataTypeName(i));
        }

        var rows = new List<IReadOnlyList<object?>>();
        bool truncated = false;

        while (reader.Read())
        {
            if (rows.Count >= maxRows)
            {
                truncated = true;
                break;
            }

            var values = new object?[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            rows.Add(values);
        }

        stopwatch.Stop();

        return new TabularResult(columns, rows, rows.Count, truncated, stopwatch.ElapsedMilliseconds);
    }
}
