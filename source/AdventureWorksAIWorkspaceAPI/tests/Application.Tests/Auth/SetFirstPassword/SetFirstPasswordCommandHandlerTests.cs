using AdventureWorksAIWorkspaceAPI.Application.Auth.SetFirstPassword;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.Auth.SetFirstPassword;

public sealed class SetFirstPasswordCommandHandlerTests
{
    private readonly IUserService _userService = Substitute.For<IUserService>();

    [Fact]
    public async Task Handle_WhenSuccess_ShouldReturnTokens()
    {
        var tokens = new AuthTokens("access", DateTime.UtcNow.AddHours(1), "refresh", DateTime.UtcNow.AddDays(7));
        _userService
            .SetFirstPasswordAsync("admin", "NewPass1!", "NewPass1!", Arg.Any<CancellationToken>())
            .Returns(new SetFirstPasswordResult(SetFirstPasswordOutcome.Success, tokens));

        var response = await SetFirstPasswordCommandHandler.Handle(
            new SetFirstPasswordCommand("admin", "NewPass1!", "NewPass1!"), _userService, CancellationToken.None);

        response.AccessToken.Should().Be("access");
        response.RefreshToken.Should().Be("refresh");
    }

    [Fact]
    public async Task Handle_WhenInvalidCredentials_ShouldThrowUnauthorizedException()
    {
        _userService
            .SetFirstPasswordAsync("unknown", "NewPass1!", "NewPass1!", Arg.Any<CancellationToken>())
            .Returns(new SetFirstPasswordResult(SetFirstPasswordOutcome.InvalidCredentials, null));

        var act = () => SetFirstPasswordCommandHandler.Handle(
            new SetFirstPasswordCommand("unknown", "NewPass1!", "NewPass1!"), _userService, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenPasswordChangeNotRequired_ShouldThrowForbiddenException()
    {
        _userService
            .SetFirstPasswordAsync("admin", "NewPass1!", "NewPass1!", Arg.Any<CancellationToken>())
            .Returns(new SetFirstPasswordResult(SetFirstPasswordOutcome.PasswordChangeNotRequired, null));

        var act = () => SetFirstPasswordCommandHandler.Handle(
            new SetFirstPasswordCommand("admin", "NewPass1!", "NewPass1!"), _userService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenPasswordChangeFailed_ShouldThrowForbiddenException()
    {
        _userService
            .SetFirstPasswordAsync("admin", "weak", "weak", Arg.Any<CancellationToken>())
            .Returns(new SetFirstPasswordResult(SetFirstPasswordOutcome.PasswordChangeFailed, null, ["Too weak."]));

        var act = () => SetFirstPasswordCommandHandler.Handle(
            new SetFirstPasswordCommand("admin", "weak", "weak"), _userService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>().WithMessage("*Too weak*");
    }
}
