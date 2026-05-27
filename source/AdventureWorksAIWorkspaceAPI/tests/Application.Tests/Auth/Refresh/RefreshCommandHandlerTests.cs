using AdventureWorksAIWorkspaceAPI.Application.Auth.Refresh;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.Auth.Refresh;

public sealed class RefreshCommandHandlerTests
{
    private readonly IUserService _userService = Substitute.For<IUserService>();

    [Fact]
    public async Task Handle_WhenSuccess_ShouldReturnNewTokens()
    {
        var tokens = new AuthTokens("new-access", DateTime.UtcNow.AddHours(1), "new-refresh", DateTime.UtcNow.AddDays(7));
        _userService
            .RefreshAsync("valid-token", Arg.Any<CancellationToken>())
            .Returns(tokens);

        var response = await RefreshCommandHandler.Handle(
            new RefreshCommand("valid-token"), _userService, CancellationToken.None);

        response.AccessToken.Should().Be("new-access");
        response.RefreshToken.Should().Be("new-refresh");
    }

    [Fact]
    public async Task Handle_WhenInvalidToken_ShouldThrowUnauthorizedException()
    {
        _userService
            .RefreshAsync("invalid-token", Arg.Any<CancellationToken>())
            .Returns((AuthTokens?)null);

        var act = () => RefreshCommandHandler.Handle(
            new RefreshCommand("invalid-token"), _userService, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
