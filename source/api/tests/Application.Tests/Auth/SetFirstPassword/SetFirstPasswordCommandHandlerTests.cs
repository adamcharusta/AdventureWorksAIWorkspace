using AdventureWorksAIWorkspace.Application.Auth.SetFirstPassword;
using AdventureWorksAIWorkspace.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.Auth;

namespace AdventureWorksAIWorkspace.Application.Tests.Auth.SetFirstPassword;

public sealed class SetFirstPasswordCommandHandlerTests
{
    private readonly IAuthenticationService _authenticationService = Substitute.For<IAuthenticationService>();

    [Fact]
    public async Task Handle_WhenSuccess_ShouldReturnTokens()
    {
        var tokens = new AuthTokens("access", DateTime.UtcNow.AddHours(1), "refresh", DateTime.UtcNow.AddDays(7));
        _authenticationService
            .SetFirstPasswordAsync("admin", "NewPass1!", "NewPass1!", Arg.Any<CancellationToken>())
            .Returns(new SetFirstPasswordResult(SetFirstPasswordOutcome.Success, tokens));

        var response = await SetFirstPasswordCommandHandler.Handle(
            new SetFirstPasswordCommand("admin", "NewPass1!", "NewPass1!"), _authenticationService, CancellationToken.None);

        response.AccessToken.Should().Be("access");
        response.RefreshToken.Should().Be("refresh");
    }

    [Fact]
    public async Task Handle_WhenInvalidCredentials_ShouldThrowUnauthorizedException()
    {
        _authenticationService
            .SetFirstPasswordAsync("unknown", "NewPass1!", "NewPass1!", Arg.Any<CancellationToken>())
            .Returns(new SetFirstPasswordResult(SetFirstPasswordOutcome.InvalidCredentials, null));

        var act = () => SetFirstPasswordCommandHandler.Handle(
            new SetFirstPasswordCommand("unknown", "NewPass1!", "NewPass1!"), _authenticationService, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenPasswordChangeNotRequired_ShouldThrowForbiddenException()
    {
        _authenticationService
            .SetFirstPasswordAsync("admin", "NewPass1!", "NewPass1!", Arg.Any<CancellationToken>())
            .Returns(new SetFirstPasswordResult(SetFirstPasswordOutcome.PasswordChangeNotRequired, null));

        var act = () => SetFirstPasswordCommandHandler.Handle(
            new SetFirstPasswordCommand("admin", "NewPass1!", "NewPass1!"), _authenticationService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenPasswordChangeFailed_ShouldThrowForbiddenException()
    {
        _authenticationService
            .SetFirstPasswordAsync("admin", "weak", "weak", Arg.Any<CancellationToken>())
            .Returns(new SetFirstPasswordResult(SetFirstPasswordOutcome.PasswordChangeFailed, null, ["Too weak."]));

        var act = () => SetFirstPasswordCommandHandler.Handle(
            new SetFirstPasswordCommand("admin", "weak", "weak"), _authenticationService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>().WithMessage("*Too weak*");
    }
}
