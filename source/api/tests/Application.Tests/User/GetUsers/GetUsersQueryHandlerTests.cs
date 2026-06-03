using AdventureWorksAIWorkspace.Application.Common.Dtos.User;
using AdventureWorksAIWorkspace.Application.Common.Services.User;
using AdventureWorksAIWorkspace.Application.User.GetUsers;

namespace AdventureWorksAIWorkspace.Application.Tests.User.GetUsers;

public sealed class GetUsersQueryHandlerTests
{
    private readonly IUserManagementService _userManagementService = Substitute.For<IUserManagementService>();

    [Fact]
    public async Task Handle_ShouldReturnUsersFromService()
    {
        var users = new List<UserDto>
        {
            new("id-1", "admin", "admin@example.com", "Admin"),
            new("id-2", "john", "john@example.com", "User")
        };
        _userManagementService
            .GetUsersAsync(Arg.Any<CancellationToken>())
            .Returns(users);

        var response = await GetUsersQueryHandler.Handle(
            new GetUsersQuery(), _userManagementService, CancellationToken.None);

        response.Users.Should().HaveCount(2);
        response.Users[0].Id.Should().Be("id-1");
        response.Users[0].UserName.Should().Be("admin");
        response.Users[1].Id.Should().Be("id-2");
        response.Users[1].Role.Should().Be("User");
    }

    [Fact]
    public async Task Handle_WhenNoUsers_ShouldReturnEmptyList()
    {
        _userManagementService
            .GetUsersAsync(Arg.Any<CancellationToken>())
            .Returns(new List<UserDto>());

        var response = await GetUsersQueryHandler.Handle(
            new GetUsersQuery(), _userManagementService, CancellationToken.None);

        response.Users.Should().BeEmpty();
    }
}
