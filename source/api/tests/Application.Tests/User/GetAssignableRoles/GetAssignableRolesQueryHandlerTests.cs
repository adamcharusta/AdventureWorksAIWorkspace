using AdventureWorksAIWorkspace.Application.Common.Services.User;
using AdventureWorksAIWorkspace.Application.User.GetAssignableRoles;

namespace AdventureWorksAIWorkspace.Application.Tests.User.GetAssignableRoles;

public sealed class GetAssignableRolesQueryHandlerTests
{
    private readonly IUserManagementService _userManagementService = Substitute.For<IUserManagementService>();

    [Fact]
    public async Task Handle_ShouldReturnRolesFromService()
    {
        var roles = new List<string> { "Admin", "User" };

        _userManagementService
            .GetAssignableRolesAsync(Arg.Any<CancellationToken>())
            .Returns(roles);

        var response = await GetAssignableRolesQueryHandler.Handle(
            new GetAssignableRolesQuery(),
            _userManagementService,
            CancellationToken.None);

        response.Roles.Should().Equal("Admin", "User");
    }

    [Fact]
    public async Task Handle_WhenNoRoles_ShouldReturnEmptyList()
    {
        _userManagementService
            .GetAssignableRolesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<string>());

        var response = await GetAssignableRolesQueryHandler.Handle(
            new GetAssignableRolesQuery(),
            _userManagementService,
            CancellationToken.None);

        response.Roles.Should().BeEmpty();
    }
}
