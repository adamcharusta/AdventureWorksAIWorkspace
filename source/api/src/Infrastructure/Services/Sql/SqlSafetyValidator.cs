using System.Text.RegularExpressions;
using AdventureWorksAIWorkspace.Application.Common.Dtos.Sql;
using AdventureWorksAIWorkspace.Application.Common.Services.Sql;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;

namespace AdventureWorksAIWorkspace.Infrastructure.Services.Sql;

/// <summary>
/// Rule-based safety validator for AI-generated SQL. Allows only single, read-only queries and
/// rejects statements that contain destructive commands.
/// </summary>
/// <remarks>
/// This is intentionally conservative: it favours false rejections over executing unsafe SQL.
/// It is not a SQL parser, so a forbidden keyword appearing inside a string literal or a bracketed
/// identifier will also be rejected. Parser-based validation and table/column allowlists are
/// future enhancements.
/// </remarks>
public sealed partial class SqlSafetyValidator : ISqlSafetyValidator
{
    private readonly ILogger<SqlSafetyValidator> _logger;

    public SqlSafetyValidator(ILogger<SqlSafetyValidator> logger)
    {
        Guard.Against.Null(logger);
        _logger = logger;
    }

    public SqlValidationResult Validate(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return Reject("SQL statement is empty.");
        }

        string sanitized = BlockCommentRegex().Replace(sql, " ");
        sanitized = LineCommentRegex().Replace(sanitized, " ").Trim();

        string[] statements = sanitized.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (statements.Length > 1)
        {
            return Reject("Multiple SQL statements are not allowed.");
        }

        string statement = statements.Length == 1 ? statements[0] : string.Empty;
        if (string.IsNullOrWhiteSpace(statement))
        {
            return Reject("SQL statement is empty.");
        }

        if (!StartsWithReadOnlyKeyword(statement))
        {
            return Reject("Only SELECT or WITH (CTE) queries are allowed.");
        }

        Match forbidden = ForbiddenKeywordRegex().Match(statement);
        if (forbidden.Success)
        {
            return Reject($"SQL contains a forbidden keyword: {forbidden.Value.ToUpperInvariant()}.");
        }

        _logger.LogDebug("SQL statement passed safety validation.");
        return SqlValidationResult.Valid();
    }

    private static bool StartsWithReadOnlyKeyword(string statement)
    {
        return statement.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
            || statement.StartsWith("WITH", StringComparison.OrdinalIgnoreCase);
    }

    private SqlValidationResult Reject(string reason)
    {
        _logger.LogWarning("Rejected unsafe SQL. Reason: {Reason}", reason);
        return SqlValidationResult.Invalid(reason);
    }

    [GeneratedRegex(
        @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|TRUNCATE|EXECUTE|EXEC|MERGE|CREATE|GRANT|REVOKE|INTO)\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex ForbiddenKeywordRegex();

    [GeneratedRegex(@"--[^\n]*")]
    private static partial Regex LineCommentRegex();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex BlockCommentRegex();
}
