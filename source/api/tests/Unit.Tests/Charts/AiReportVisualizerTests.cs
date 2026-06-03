using AdventureWorksAIWorkspace.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspace.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspace.Application.Common.Dtos.Charts;
using AdventureWorksAIWorkspace.Application.Common.Services.Ai;
using AdventureWorksAIWorkspace.Infrastructure.Services.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute.ExceptionExtensions;

namespace AdventureWorksAIWorkspace.Unit.Tests.Charts;

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
    public async Task CreatePresentationAsync_ShouldParseConclusionsWhenProvided()
    {
        SetModelResponse(
            """
            {
              "insights": "Revenue grew month over month.",
              "conclusions": "The trend is clearly upward; if it holds, next year would be well above this one.",
              "charts": []
            }
            """);

        ReportPresentation presentation = await CreateVisualizer().CreatePresentationAsync("revenue", SampleResult());

        presentation.Conclusions.Should().Contain("trend is clearly upward");
    }

    [Fact]
    public async Task CreatePresentationAsync_ShouldLeaveConclusionsNullWhenAbsentOrEmpty()
    {
        SetModelResponse("{ \"insights\": \"ok\", \"conclusions\": \"   \", \"charts\": [] }");

        ReportPresentation presentation = await CreateVisualizer().CreatePresentationAsync("q", SampleResult());

        // An absent or whitespace-only value must stay null so the UI renders nothing extra.
        presentation.Conclusions.Should().BeNull();
    }

    [Fact]
    public async Task CreatePresentationAsync_ShouldNotFabricateConclusionsInFallback()
    {
        SetModelResponse("this is not json at all");

        ReportPresentation presentation = await CreateVisualizer().CreatePresentationAsync("q", SampleResult());

        presentation.Conclusions.Should().BeNull();
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
    public async Task CreatePresentationAsync_ShouldAskModelToUseQuestionLanguageForUserFacingText()
    {
        IReadOnlyList<AiChatMessage>? capturedMessages = null;
        _chatClient
            .CompleteAsync(
                Arg.Do<IReadOnlyList<AiChatMessage>>(messages => capturedMessages = messages),
                Arg.Any<CancellationToken>())
            .Returns(new AiChatResult("{ \"insights\": \"ok\", \"charts\": [] }", 10, 20));

        await CreateVisualizer().CreatePresentationAsync("Show sales by category", SampleResult());

        capturedMessages.Should().NotBeNull();
        capturedMessages!.Should().HaveCount(2);
        capturedMessages[0].Role.Should().Be(AiChatRole.System);
        capturedMessages[0].Content.Should().Contain("same language as the user's question");
        capturedMessages[0].Content.Should().Contain("Keep JSON property names and result column names unchanged");
        capturedMessages[1].Role.Should().Be(AiChatRole.User);
        capturedMessages[1].Content.Should().Contain("Show sales by category");
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
