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
        _chatClient
            .CompleteAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>())
            .Returns(new AiChatResult("SELECT 1", null, null));

        var generator = new AiSqlGenerator(_chatClient);

        await generator.GenerateSqlAsync("show me total sales by territory");

        await _chatClient.Received(1).CompleteAsync(
            Arg.Is<IReadOnlyList<AiChatMessage>>(messages =>
                messages.Count == 2 &&
                messages[0].Role == AiChatRole.System &&
                messages[1].Role == AiChatRole.User &&
                messages[1].Content == "show me total sales by territory"),
            Arg.Any<CancellationToken>());
    }
}
