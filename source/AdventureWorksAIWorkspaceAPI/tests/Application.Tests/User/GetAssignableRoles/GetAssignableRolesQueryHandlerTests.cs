using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.User.GetAssignableRoles;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.User.GetAssignableRoles;

public sealed class GetAssignableRolesQueryHandlerTests
{
    private readonly IUserService _userService = Substitute.For<IUserService>();

    [Fact]
    public async Task Handle_ShouldReturnRolesFromService()
    {
        var roles = new List<string> { "Admin", "User" };

        _userService
            .GetAssignableRolesAsync(Arg.Any<CancellationToken>())
            .Returns(roles);

        var response = await GetAssignableRolesQueryHandler.Handle(
            new GetAssignableRolesQuery(),
            _userService,
            CancellationToken.None);

        response.Roles.Should().Equal("Admin", "User");
    }

    [Fact]
    public async Task Handle_WhenNoRoles_ShouldReturnEmptyList()
    {
        _userService
            .GetAssignableRolesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<string>());

        var response = await GetAssignableRolesQueryHandler.Handle(
            new GetAssignableRolesQuery(),
            _userService,
            CancellationToken.None);

        response.Roles.Should().BeEmpty();
    }
}
