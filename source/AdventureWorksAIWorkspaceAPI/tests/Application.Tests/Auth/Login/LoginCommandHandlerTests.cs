using AdventureWorksAIWorkspaceAPI.Application.Auth.Login;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.Auth.Login;

public sealed class LoginCommandHandlerTests
{
    private readonly IUserService _userService = Substitute.For<IUserService>();

    [Fact]
    public async Task Handle_WhenSuccess_ShouldReturnTokens()
    {
        var tokens = new AuthTokens("access", DateTime.UtcNow.AddHours(1), "refresh", DateTime.UtcNow.AddDays(7));
        _userService
            .LoginAsync("admin", "password", Arg.Any<CancellationToken>())
            .Returns(new LoginResult(LoginOutcome.Success, tokens));

        var response = await LoginCommandHandler.Handle(
            new LoginCommand("admin", "password"), _userService, CancellationToken.None);

        response.AccessToken.Should().Be("access");
        response.RefreshToken.Should().Be("refresh");
    }

    [Fact]
    public async Task Handle_WhenInvalidCredentials_ShouldThrowUnauthorizedException()
    {
        _userService
            .LoginAsync("admin", "wrong", Arg.Any<CancellationToken>())
            .Returns(new LoginResult(LoginOutcome.InvalidCredentials, null));

        var act = () => LoginCommandHandler.Handle(
            new LoginCommand("admin", "wrong"), _userService, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenPasswordChangeRequired_ShouldThrowForbiddenException()
    {
        _userService
            .LoginAsync("admin", "password", Arg.Any<CancellationToken>())
            .Returns(new LoginResult(LoginOutcome.PasswordChangeRequired, null));

        var act = () => LoginCommandHandler.Handle(
            new LoginCommand("admin", "password"), _userService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
