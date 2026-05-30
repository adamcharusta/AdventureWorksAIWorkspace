using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Infrastructure.Services;

namespace AdventureWorksAIWorkspaceAPI.Unit.Tests.Ai;

public sealed class AiSqlGeneratorTests
{
    private readonly IAiChatClient _chatClient = Substitute.For<IAiChatClient>();

    [Theory]
    [InlineData("SELECT 1", "SELECT 1")]
    [InlineData("  SELECT 1  ", "SELECT 1")]
    [InlineData("```sql\nSELECT 1\n```", "SELECT 1")]
    [InlineData("```\nSELECT 1\n```", "SELECT 1")]
    [InlineData("Here is the query:\n```sql\nSELECT 1\n```", "SELECT 1")]
    public async Task GenerateSqlAsync_ShouldStripCodeFencesAndReturnSql(string content, string expected)
    {
        _chatClient
            .CompleteAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new AiChatResult(content, 11, 22));

        var generator = new AiSqlGenerator(_chatClient);

        GeneratedSql result = await generator.GenerateSqlAsync("question");

        result.Sql.Should().Be(expected);
        result.InputTokens.Should().Be(11);
        result.OutputTokens.Should().Be(22);
    }

    [Fact]
    public async Task GenerateSqlAsync_ShouldSendSystemAndUserMessages()
    {
        IReadOnlyList<AiChatMessage>? capturedMessages = null;
        _chatClient
            .CompleteAsync(
                Arg.Do<IReadOnlyList<AiChatMessage>>(messages => capturedMessages = messages),
                Arg.Any<CancellationToken>())
            .Returns(new AiChatResult("SELECT 1", null, null));

        var generator = new AiSqlGenerator(_chatClient);

        await generator.GenerateSqlAsync("show me total sales by territory");

        capturedMessages.Should().NotBeNull();
        capturedMessages!.Should().HaveCount(2);
        capturedMessages[0].Role.Should().Be(AiChatRole.System);
        capturedMessages[0].Content.Should().Contain("Sales.SalesOrderHeader");
        capturedMessages[0].Content.Should().Contain("Sales.SalesOrderDetail");
        capturedMessages[1].Role.Should().Be(AiChatRole.User);
        capturedMessages[1].Content.Should().Contain("Business question:");
        capturedMessages[1].Content.Should().Contain("show me total sales by territory");

        await _chatClient.Received(1).CompleteAsync(
            Arg.Any<IReadOnlyList<AiChatMessage>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateSqlAsync_WhenContextIsProvided_ShouldIncludeReportContext()
    {
        IReadOnlyList<AiChatMessage>? capturedMessages = null;
        _chatClient
            .CompleteAsync(
                Arg.Do<IReadOnlyList<AiChatMessage>>(messages => capturedMessages = messages),
                Arg.Any<CancellationToken>())
            .Returns(new AiChatResult("SELECT 1", null, null));

        var context = new AiSqlGenerationContext(
            "2013 sales overview",
            "Bikes dominate revenue.",
            ["User: 2013 sales overview", "Assistant: Bikes dominate revenue."],
            "SELECT TOP (10) SalesOrderID FROM Sales.SalesOrderHeader");
        var generator = new AiSqlGenerator(_chatClient);

        await generator.GenerateSqlAsync("show monthly trend", context);

        capturedMessages.Should().NotBeNull();
        capturedMessages![1].Content.Should().Contain("Business question:");
        capturedMessages[1].Content.Should().Contain("show monthly trend");
        capturedMessages[1].Content.Should().Contain("Original report prompt:");
        capturedMessages[1].Content.Should().Contain("2013 sales overview");
        capturedMessages[1].Content.Should().Contain("Current report summary:");
        capturedMessages[1].Content.Should().Contain("Recent conversation:");
        capturedMessages[1].Content.Should().Contain("Last successful SQL:");
        capturedMessages[1].Content.Should().Contain("Sales.SalesOrderHeader");
    }
}
