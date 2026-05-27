using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;

namespace AdventureWorksAIWorkspaceAPI.Application.User.GetUsers;

public sealed record GetUsersQuery;

public static class GetUsersQueryHandler
{
    public static async Task<GetUsersResponse> Handle(
        GetUsersQuery query,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<UserDto> users = await userService.GetUsersAsync(cancellationToken);
        return new GetUsersResponse(users);
    }
}
