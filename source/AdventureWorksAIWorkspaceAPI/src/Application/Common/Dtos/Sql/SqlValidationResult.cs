namespace AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Sql;

/// <summary>
/// The outcome of validating a SQL statement before it is executed against AdventureWorks.
/// </summary>
/// <param name="IsValid">True when the statement is considered safe to execute.</param>
/// <param name="Reason">A human-readable reason when the statement is rejected; otherwise null.</param>
public sealed record SqlValidationResult(bool IsValid, string? Reason)
{
    /// <summary>Creates a successful validation result.</summary>
    public static SqlValidationResult Valid() => new(true, null);

    /// <summary>Creates a rejection result with the given reason.</summary>
    public static SqlValidationResult Invalid(string reason) => new(false, reason);
}
