using AdventureWorksAIWorkspace.Application.Common.Dtos.User;
using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.User;
using AdventureWorksAIWorkspace.Application.User.UpdateUser;

namespace AdventureWorksAIWorkspace.Application.Tests.User.UpdateUser;

public sealed class UpdateUserCommandHandlerTests
{
    private readonly IUserManagementService _userManagementService = Substitute.For<IUserManagementService>();

    [Fact]
    public async Task Handle_WhenSuccess_ShouldReturnUpdatedUser()
    {
        var command = new UpdateUserCommand("id-1", "newname", "new@example.com", "Admin");
        _userManagementService
            .UpdateUserAsync(command, Arg.Any<CancellationToken>())
            .Returns(new UpdateUserResult(UpdateUserOutcome.Success, "id-1", "newname", "new@example.com", "Admin"));

        var response = await UpdateUserCommandHandler.Handle(command, _userManagementService, CancellationToken.None);

        response.UserId.Should().Be("id-1");
        response.UserName.Should().Be("newname");
        response.Email.Should().Be("new@example.com");
        response.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        var command = new UpdateUserCommand("missing-id");
        _userManagementService
            .UpdateUserAsync(command, Arg.Any<CancellationToken>())
            .Returns(new UpdateUserResult(UpdateUserOutcome.UserNotFound));

        var act = () => UpdateUserCommandHandler.Handle(command, _userManagementService, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenInvalidRole_ShouldThrowForbiddenException()
    {
        var command = new UpdateUserCommand("id-1", Role: "Invalid");
        _userManagementService
            .UpdateUserAsync(command, Arg.Any<CancellationToken>())
            .Returns(new UpdateUserResult(UpdateUserOutcome.InvalidRole));

        var act = () => UpdateUserCommandHandler.Handle(command, _userManagementService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenUserNameAlreadyTaken_ShouldThrowForbiddenException()
    {
        var command = new UpdateUserCommand("id-1", UserName: "taken");
        _userManagementService
            .UpdateUserAsync(command, Arg.Any<CancellationToken>())
            .Returns(new UpdateUserResult(UpdateUserOutcome.UserNameAlreadyTaken));

        var act = () => UpdateUserCommandHandler.Handle(command, _userManagementService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>().WithMessage("*taken*");
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyTaken_ShouldThrowForbiddenException()
    {
        var command = new UpdateUserCommand("id-1", Email: "taken@example.com");
        _userManagementService
            .UpdateUserAsync(command, Arg.Any<CancellationToken>())
            .Returns(new UpdateUserResult(UpdateUserOutcome.EmailAlreadyTaken));

        var act = () => UpdateUserCommandHandler.Handle(command, _userManagementService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>().WithMessage("*taken@example.com*");
    }

    [Fact]
    public async Task Handle_WhenUpdateFailed_ShouldThrowForbiddenException()
    {
        var command = new UpdateUserCommand("id-1", UserName: "newname");
        _userManagementService
            .UpdateUserAsync(command, Arg.Any<CancellationToken>())
            .Returns(new UpdateUserResult(UpdateUserOutcome.UpdateFailed, Errors: ["Concurrency error."]));

        var act = () => UpdateUserCommandHandler.Handle(command, _userManagementService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>().WithMessage("*Concurrency error*");
    }
}
