using System.Text.Json;

namespace AdventureWorksAIWorkspace.Application.Reports;

/// <summary>
/// Shared JSON options for persisting and reading the report result and chart snapshots that are
/// stored as JSON columns. Centralized so serialization and deserialization stay symmetric.
/// </summary>
internal static class ReportJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
