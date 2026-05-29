namespace AdventureWorksAIWorkspaceAPI.Infrastructure.OpenAi;

/// <summary>
/// Options that control how the application talks to the OpenAI API.
/// </summary>
/// <remarks>
/// The API key must never be committed to source control. Supply it through development User
/// Secrets or environment variables. See the technical decision "Use the official OpenAI .NET
/// SDK behind an Application abstraction for AI features".
/// </remarks>
public class OpenAiOptions
{
    public const string SectionName = "OpenAi";

    /// <summary>The OpenAI API key. Supplied through secrets, not committed configuration.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>The model used for chat completions.</summary>
    public string Model { get; set; } = "gpt-4o";

    /// <summary>
    /// Optional base URL override, for example when targeting an OpenAI-compatible endpoint.
    /// When empty, the SDK default endpoint is used.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>Per-request timeout, in seconds, applied to the underlying HTTP client.</summary>
    public int TimeoutSeconds { get; set; } = 100;
}
