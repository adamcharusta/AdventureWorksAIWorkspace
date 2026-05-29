namespace AdventureWorksAIWorkspaceAPI.Infrastructure.AdventureWorks;

/// <summary>
/// Options that control how read-only AdventureWorks queries are executed.
/// </summary>
public class AdventureWorksQueryOptions
{
    public const string SectionName = "AdventureWorks";

    /// <summary>
    /// Maximum time, in seconds, a single AdventureWorks query may run before it is cancelled.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of rows returned for a single query. Additional rows are dropped and the
    /// result is marked as truncated.
    /// </summary>
    public int MaxRows { get; set; } = 1000;
}
