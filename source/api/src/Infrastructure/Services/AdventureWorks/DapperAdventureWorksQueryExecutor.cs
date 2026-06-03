using System.Data;
using AdventureWorksAIWorkspace.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.AdventureWorks;
using AdventureWorksAIWorkspace.Infrastructure.AdventureWorks;
using Ardalis.GuardClauses;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AdventureWorksAIWorkspace.Infrastructure.Services.AdventureWorks;

/// <summary>
/// Executes read-only SQL against the AdventureWorks analytical database using Dapper over a
/// SQL Server connection.
/// </summary>
/// <remarks>
/// The connection string must use read-only credentials. The statement is expected to have
/// already passed safety validation; this executor enforces only the command timeout and the
/// configured row limit.
/// </remarks>
public sealed class DapperAdventureWorksQueryExecutor : IAdventureWorksQueryExecutor
{
    private readonly string _connectionString;
    private readonly AdventureWorksQueryOptions _options;

    public DapperAdventureWorksQueryExecutor(string connectionString, AdventureWorksQueryOptions options)
    {
        Guard.Against.NullOrWhiteSpace(connectionString);
        Guard.Against.Null(options);

        _connectionString = connectionString;
        _options = options;
    }

    public async Task<TabularResult> ExecuteQueryAsync(string sql, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(sql);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            commandTimeout: _options.CommandTimeoutSeconds,
            cancellationToken: cancellationToken);

        try
        {
            using IDataReader reader = await connection.ExecuteReaderAsync(command);

            return TabularResultReader.Read(reader, _options.MaxRows);
        }
        catch (SqlException ex)
        {
            // Translate query-level failures (invalid object name, timeout, bad SQL produced by
            // the model) into an application exception. Connection-level failures happen during
            // OpenAsync above and are intentionally left to surface as infrastructure errors.
            throw new QueryExecutionException($"Query execution failed: {ex.Message}", ex);
        }
    }
}
