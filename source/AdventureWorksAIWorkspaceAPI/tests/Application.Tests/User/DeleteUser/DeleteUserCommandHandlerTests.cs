using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.User.DeleteUser;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.User.DeleteUser;

public sealed class DeleteUserCommandHandlerTests
{
    private readonly IUserManagementService _userManagementService = Substitute.For<IUserManagementService>();

    [Fact]
    public async Task Handle_WhenSuccess_ShouldDeleteUser()
    {
        var command = new DeleteUserCommand("target-id", "admin-id");
        _userManagementService
            .DeleteUserAsync("target-id", Arg.Any<CancellationToken>())
            .Returns(new DeleteUserResult(DeleteUserOutcome.Success));

        await DeleteUserCommandHandler.Handle(command, _userManagementService, CancellationToken.None);

        _ = _userManagementService.Received(1).DeleteUserAsync("target-id", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Handle_WhenCurrentUserIdIsMissing_ShouldThrowUnauthorizedException(string? currentUserId)
    {
        var command = new DeleteUserCommand("target-id", currentUserId);

        var act = () => DeleteUserCommandHandler.Handle(command, _userManagementService, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
        _ = _userManagementService.DidNotReceive().DeleteUserAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCurrentUserDeletesSelf_ShouldThrowForbiddenException()
    {
        var command = new DeleteUserCommand("admin-id", "admin-id");

        var act = () => DeleteUserCommandHandler.Handle(command, _userManagementService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>().WithMessage("*own user account*");
        _ = _userManagementService.DidNotReceive().DeleteUserAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        var command = new DeleteUserCommand("missing-id", "admin-id");
        _userManagementService
            .DeleteUserAsync("missing-id", Arg.Any<CancellationToken>())
            .Returns(new DeleteUserResult(DeleteUserOutcome.UserNotFound));

        var act = () => DeleteUserCommandHandler.Handle(command, _userManagementService, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*missing-id*");
    }

    [Fact]
    public async Task Handle_WhenDeleteFails_ShouldThrowForbiddenException()
    {
        var command = new DeleteUserCommand("target-id", "admin-id");
        _userManagementService
            .DeleteUserAsync("target-id", Arg.Any<CancellationToken>())
            .Returns(new DeleteUserResult(DeleteUserOutcome.DeleteFailed, ["Delete failed."]));

        var act = () => DeleteUserCommandHandler.Handle(command, _userManagementService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>().WithMessage("*Delete failed*");
    }
}
