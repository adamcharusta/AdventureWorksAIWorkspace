using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.User.CreateUser;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.User.CreateUser;

public sealed class CreateUserCommandHandlerTests
{
    private readonly IUserService _userService = Substitute.For<IUserService>();

    [Fact]
    public async Task Handle_WhenSuccess_ShouldReturnCreatedUser()
    {
        _userService
            .CreateUserAsync("john", "john@example.com", "User", Arg.Any<CancellationToken>())
            .Returns(new CreateUserResult(CreateUserOutcome.Success, "id-1", "john", "john@example.com", "User"));

        var response = await CreateUserCommandHandler.Handle(
            new CreateUserCommand("john", "john@example.com", "User"), _userService, CancellationToken.None);

        response.UserId.Should().Be("id-1");
        response.UserName.Should().Be("john");
        response.Email.Should().Be("john@example.com");
        response.Role.Should().Be("User");
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ShouldThrowForbiddenException()
    {
        _userService
            .CreateUserAsync("john", "john@example.com", null, Arg.Any<CancellationToken>())
            .Returns(new CreateUserResult(CreateUserOutcome.UserAlreadyExists));

        var act = () => CreateUserCommandHandler.Handle(
            new CreateUserCommand("john", "john@example.com"), _userService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenInvalidRole_ShouldThrowForbiddenException()
    {
        _userService
            .CreateUserAsync("john", "john@example.com", "Invalid", Arg.Any<CancellationToken>())
            .Returns(new CreateUserResult(CreateUserOutcome.InvalidRole));

        var act = () => CreateUserCommandHandler.Handle(
            new CreateUserCommand("john", "john@example.com", "Invalid"), _userService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>().WithMessage("*Invalid*");
    }

    [Fact]
    public async Task Handle_WhenCreationFailed_ShouldThrowForbiddenException()
    {
        _userService
            .CreateUserAsync("john", "john@example.com", null, Arg.Any<CancellationToken>())
            .Returns(new CreateUserResult(CreateUserOutcome.CreationFailed, Errors: ["Something went wrong."]));

        var act = () => CreateUserCommandHandler.Handle(
            new CreateUserCommand("john", "john@example.com"), _userService, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>().WithMessage("*Something went wrong*");
    }
}
