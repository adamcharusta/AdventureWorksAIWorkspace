using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Charts;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute.ExceptionExtensions;

namespace AdventureWorksAIWorkspaceAPI.Unit.Tests.Charts;

public sealed class AiReportVisualizerTests
{
    private readonly IAiChatClient _chatClient = Substitute.For<IAiChatClient>();

    private static TabularResult SampleResult() => new(
        [new TabularColumn("Month", "nvarchar"), new TabularColumn("Revenue", "decimal")],
        [["Jan", 100m], ["Feb", 150m]],
        2,
        false,
        5);

    private AiReportVisualizer CreateVisualizer() =>
        new(_chatClient, NullLogger<AiReportVisualizer>.Instance);

    private void SetModelResponse(string content) =>
        _chatClient
            .CompleteAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new AiChatResult(content, 10, 20));

    [Fact]
    public async Task CreatePresentationAsync_ShouldParseInsightsAndCharts()
    {
        SetModelResponse(
            """
            {
              "title": "Monthly revenue trend",
              "insights": "Revenue grew from January to February.",
              "charts": [
                { "kind": "line", "title": "Revenue by month", "categoryColumn": "Month",
                  "series": [ { "column": "Revenue", "label": "Revenue" } ], "description": "Monthly revenue" }
              ]
            }
            """);

        ReportPresentation presentation = await CreateVisualizer().CreatePresentationAsync("revenue", SampleResult());

        presentation.Title.Should().Be("Monthly revenue trend");
        presentation.Insights.Should().Contain("Revenue grew");
        presentation.Charts.Should().ContainSingle();
        presentation.Charts[0].Kind.Should().Be(ChartKind.Line);
        presentation.Charts[0].CategoryColumn.Should().Be("Month");
        presentation.Charts[0].Series.Should().ContainSingle(s => s.Column == "Revenue");
    }

    [Fact]
    public async Task CreatePresentationAsync_ShouldStripCodeFencesAroundJson()
    {
        SetModelResponse("```json\n{ \"insights\": \"ok\", \"charts\": [] }\n```");

        ReportPresentation presentation = await CreateVisualizer().CreatePresentationAsync("q", SampleResult());

        presentation.Insights.Should().Be("ok");
        presentation.Charts.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePresentationAsync_ShouldDropChartsReferencingUnknownColumns()
    {
        SetModelResponse(
            """
            {
              "insights": "i",
              "charts": [
                { "kind": "bar", "title": "Bad", "categoryColumn": "Month",
                  "series": [ { "column": "DoesNotExist", "label": "x" } ] }
              ]
            }
            """);

        ReportPresentation presentation = await CreateVisualizer().CreatePresentationAsync("q", SampleResult());

        // The only series referenced a non-existent column, so the non-table chart is dropped.
        presentation.Charts.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePresentationAsync_ShouldFallBackWhenJsonIsInvalid()
    {
        SetModelResponse("this is not json at all");

        ReportPresentation presentation = await CreateVisualizer().CreatePresentationAsync("q", SampleResult());

        // Heuristic fallback: a non-numeric category column + a numeric series -> bar chart.
        presentation.Insights.Should().NotBeNullOrWhiteSpace();
        presentation.Charts.Should().ContainSingle();
        presentation.Charts[0].Kind.Should().Be(ChartKind.Bar);
        presentation.Charts[0].CategoryColumn.Should().Be("Month");
        presentation.Charts[0].Series.Should().ContainSingle(s => s.Column == "Revenue");
    }

    [Fact]
    public async Task CreatePresentationAsync_ShouldFallBackWhenModelThrows()
    {
        _chatClient
            .CompleteAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("model down"));

        ReportPresentation presentation = await CreateVisualizer().CreatePresentationAsync("q", SampleResult());

        presentation.Charts.Should().ContainSingle();
        presentation.Insights.Should().NotBeNullOrWhiteSpace();
    }
}
