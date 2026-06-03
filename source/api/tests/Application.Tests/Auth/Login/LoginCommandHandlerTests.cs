using AdventureWorksAIWorkspace.Application.Auth.Login;
using AdventureWorksAIWorkspace.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.Auth;

namespace AdventureWorksAIWorkspace.Application.Tests.Auth.Login;

public sealed class LoginCommandHandlerTests
{
    private readonly IAuthenticationService _authenticationService = Substitute.For<IAuthenticationService>();

    [Fact]
    public async Task Handle_WhenSuccess_ShouldReturnTokens()
    {
        var tokens = new AuthTokens("access", DateTime.UtcNow.AddHours(1), "refresh", DateTime.UtcNow.AddDays(7));
        _authenticationService
            .LoginAsync("admin", "password", Arg.Any<CancellationToken>())
            .Returns(new LoginResult(LoginOutcome.Success, tokens));

        var response = await LoginCommandHandler.Handle(
            new LoginCommand("admin", "password"), _authenticationService, CancellationToken.None);

        response.AccessToken.Should().Be("access");
        response.RefreshToken.Should().Be("refresh");
    }

    [Fact]
    public async Task Handle_WhenInvalidCredentials_ShouldThrowUnauthorizedException()
    {
        _authenticationService
            .LoginAsync("admin", "wrong", Arg.Any<CancellationToken>())
            .Returns(new LoginResult(LoginOutcome.InvalidCredentials, null));

        var act = () => LoginCommandHandler.Handle(
            new LoginCommand("admin", "wrong"), _authenticationService, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenPasswordChangeRequired_ShouldThrowForbiddenException()
    {
        _authenticationService
            .LoginAsync("admin", "password", Arg.Any<CancellationToken>())
            .Returns(new LoginResult(LoginOutcome.PasswordChangeRequired, null));

        var act = () => LoginCommandHandler.Handle(
            new LoginCommand("admin", "password"), _authenticationService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
