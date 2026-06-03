using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AdventureWorksAIWorkspace.Infrastructure.Services.Ai;

/// <summary>
/// Shared helpers for turning raw AI chat completions into usable text or JSON. The model often
/// wraps its answer in a markdown code fence (```sql ... ``` or ```json ... ```); these helpers
/// strip that wrapping consistently so each AI service does not re-implement it.
/// </summary>
internal static partial class AiResponseParser
{
    /// <summary>
    /// JSON options for parsing model output: case-insensitive property names and camelCase string
    /// enums (the model emits values such as "bar"/"line").
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Removes a surrounding markdown code fence (with an optional language tag) and trims the
    /// result. Returns the trimmed input unchanged when no fence is present.
    /// </summary>
    public static string StripCodeFence(string? content)
    {
        string text = (content ?? string.Empty).Trim();
        Match fence = CodeFenceRegex().Match(text);
        return fence.Success ? fence.Groups["body"].Value.Trim() : text;
    }

    /// <summary>
    /// Strips any code fence and then narrows to the outermost JSON object (from the first <c>{</c>
    /// to the last <c>}</c>), so trailing prose around a JSON answer does not break parsing.
    /// </summary>
    public static string ExtractJsonObject(string? content)
    {
        string text = StripCodeFence(content);
        int start = text.IndexOf('{');
        int end = text.LastIndexOf('}');
        return start >= 0 && end > start ? text.Substring(start, end - start + 1) : text;
    }

    [GeneratedRegex("```(?:[a-zA-Z0-9]+)?\\s*(?<body>.+?)```", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex CodeFenceRegex();
}
