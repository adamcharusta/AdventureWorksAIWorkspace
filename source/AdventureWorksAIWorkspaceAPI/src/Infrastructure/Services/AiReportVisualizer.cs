using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Charts;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Services;

/// <summary>
/// Asks the AI model to choose charts and write insights for a report result, then validates the
/// model output against the actual result columns. Falls back to a deterministic, type-based
/// presentation when the model output is missing or unusable, so report generation never fails
/// because of visualization.
/// </summary>
public sealed class AiReportVisualizer : IReportVisualizer
{
    private const int MaxSampleRows = 15;

    private static readonly string[] NumericDataTypes =
    [
        "int", "bigint", "smallint", "tinyint", "decimal", "numeric",
        "money", "smallmoney", "float", "real"
    ];

    private const string SystemPrompt =
        """
        You are a data visualization assistant for a Microsoft AdventureWorks business intelligence tool.
        Given the user's question and the result set of a SQL query (columns with types and a sample of rows),
        choose the most useful charts and write concise business insights.

        Respond with ONLY a JSON object (no markdown, no code fences) of this exact shape:
        {
          "title": "a short report title, at most 8 words, no quotes",
          "insights": "2-4 sentences of plain-text business insights",
          "conclusions": "optional deeper analysis; omit or use null when you have nothing extra to add",
          "charts": [
            {
              "kind": "bar | line | pie | area | table",
              "title": "short chart title",
              "categoryColumn": "a column name for the x-axis or pie labels, or null",
              "series": [ { "column": "a numeric column name", "label": "optional label" } ],
              "description": "optional one-line description"
            }
          ]
        }

        Rules:
        - Only reference column names that appear in the provided columns.
        - "series" columns must be numeric; "categoryColumn" is the grouping/label column.
        - Write every user-facing value in the same language as the user's question:
          report title, insights, conclusions, chart titles, chart descriptions, and series labels.
        - Keep JSON property names and result column names unchanged. If the question language is unclear or mixed, use English.
        - Use "line" or "area" for trends over a date/time column, "bar" for category comparisons,
          "pie" for parts of a whole with few categories, and "table" when no chart is meaningful.
        - Prefer one or two charts. If the data is not chartable, return a single "table" chart and still provide insights.

        About "conclusions" (optional):
        - "insights" describes what the result shows. "conclusions" goes further: it interprets a
          trend the chart reveals, projects where it is heading, flags a caveat, or recommends a
          next step. For example: "The chart shows a clear upward trend; if it holds, revenue in
          2035 would be roughly 150% of today's level, so it is worth planning capacity ahead."
        - Only include "conclusions" when it adds genuine value beyond the insights. When you have
          nothing meaningful to add, omit the property or set it to null. Do not repeat the insights.
        - Never put SQL or commands in "conclusions"; it is advisory prose only.
        """;

    private readonly IAiChatClient _chatClient;
    private readonly ILogger<AiReportVisualizer> _logger;

    public AiReportVisualizer(IAiChatClient chatClient, ILogger<AiReportVisualizer> logger)
    {
        Guard.Against.Null(chatClient);
        Guard.Against.Null(logger);

        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<ReportPresentation> CreatePresentationAsync(
        string question,
        TabularResult result,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(result);

        try
        {
            IReadOnlyList<AiChatMessage> messages =
            [
                AiChatMessage.System(SystemPrompt),
                AiChatMessage.User(BuildUserPrompt(question, result))
            ];

            AiChatResult completion = await _chatClient.CompleteAsync(messages, cancellationToken);

            ReportPresentation? presentation = TryParse(completion.Content, result);
            if (presentation is not null)
            {
                return presentation;
            }

            _logger.LogWarning("AI visualization output could not be parsed; using heuristic fallback.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "AI visualization request failed; using heuristic fallback.");
        }

        return BuildFallback(result);
    }

    private static string BuildUserPrompt(string question, TabularResult result)
    {
        var builder = new StringBuilder();
        builder.Append("Question: ").AppendLine(question);
        builder.AppendLine();
        builder.AppendLine("Columns:");
        foreach (TabularColumn column in result.Columns)
        {
            builder.Append("- ").Append(column.Name).Append(" (").Append(column.DataType).AppendLine(")");
        }

        builder.AppendLine();
        builder.Append("Sample rows (JSON, up to ").Append(MaxSampleRows).AppendLine(" rows):");

        IEnumerable<IReadOnlyList<object?>> sample = result.Rows.Take(MaxSampleRows);
        builder.Append(JsonSerializer.Serialize(sample));

        return builder.ToString();
    }

    private ReportPresentation? TryParse(string content, TabularResult result)
    {
        string json = AiResponseParser.ExtractJsonObject(content);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        PresentationJson? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<PresentationJson>(json, AiResponseParser.JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        if (parsed is null)
        {
            return null;
        }

        string insights = string.IsNullOrWhiteSpace(parsed.Insights)
            ? DefaultInsights(result)
            : parsed.Insights.Trim();

        var columnNames = result.Columns
            .Select(column => column.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var charts = new List<ChartSpec>();
        foreach (ChartJson chart in parsed.Charts ?? [])
        {
            ChartSpec? spec = MapChart(chart, columnNames);
            if (spec is not null)
            {
                charts.Add(spec);
            }
        }

        string? title = string.IsNullOrWhiteSpace(parsed.Title) ? null : parsed.Title.Trim();

        // Conclusions are optional: an absent/empty value is normal and must stay null so the
        // UI renders nothing extra. The fallback path never fabricates conclusions.
        string? conclusions = string.IsNullOrWhiteSpace(parsed.Conclusions) ? null : parsed.Conclusions.Trim();

        return new ReportPresentation(insights, charts, title, conclusions);
    }

    private static ChartSpec? MapChart(ChartJson chart, HashSet<string> columnNames)
    {
        ChartKind kind = ParseKind(chart.Kind);

        string? categoryColumn = columnNames.Contains(chart.CategoryColumn ?? string.Empty)
            ? chart.CategoryColumn
            : null;

        var series = (chart.Series ?? [])
            .Where(s => !string.IsNullOrWhiteSpace(s.Column) && columnNames.Contains(s.Column!))
            .Select(s => new ChartSeries(s.Column!, s.Label))
            .ToList();

        // A non-table chart needs at least one valid numeric series to be renderable.
        if (kind != ChartKind.Table && series.Count == 0)
        {
            return null;
        }

        string title = string.IsNullOrWhiteSpace(chart.Title) ? "Report chart" : chart.Title.Trim();

        return new ChartSpec(kind, title, categoryColumn, series, chart.Description?.Trim());
    }

    private static ChartKind ParseKind(string? kind) =>
        Enum.TryParse(kind, ignoreCase: true, out ChartKind parsed) ? parsed : ChartKind.Table;

    private static ReportPresentation BuildFallback(TabularResult result)
    {
        string insights = DefaultInsights(result);

        string? categoryColumn = result.Columns
            .FirstOrDefault(column => !IsNumeric(column.DataType))?.Name;

        var series = result.Columns
            .Where(column => IsNumeric(column.DataType))
            .Select(column => new ChartSeries(column.Name, column.Name))
            .ToList();

        ChartSpec chart = categoryColumn is not null && series.Count > 0
            ? new ChartSpec(ChartKind.Bar, "Report chart", categoryColumn, series, null)
            : new ChartSpec(ChartKind.Table, "Report data", null, [], null);

        return new ReportPresentation(insights, [chart]);
    }

    private static string DefaultInsights(TabularResult result) =>
        $"The query returned {result.RowCount} row(s) across {result.Columns.Count} column(s).";

    private static bool IsNumeric(string dataType) =>
        NumericDataTypes.Contains(dataType, StringComparer.OrdinalIgnoreCase);

    private sealed record PresentationJson(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("insights")] string? Insights,
        [property: JsonPropertyName("conclusions")] string? Conclusions,
        [property: JsonPropertyName("charts")] List<ChartJson>? Charts);

    private sealed record ChartJson(
        [property: JsonPropertyName("kind")] string? Kind,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("categoryColumn")] string? CategoryColumn,
        [property: JsonPropertyName("series")] List<SeriesJson>? Series,
        [property: JsonPropertyName("description")] string? Description);

    private sealed record SeriesJson(
        [property: JsonPropertyName("column")] string? Column,
        [property: JsonPropertyName("label")] string? Label);
}
