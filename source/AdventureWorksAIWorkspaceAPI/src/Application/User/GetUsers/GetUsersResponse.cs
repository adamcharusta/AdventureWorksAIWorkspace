using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;

namespace AdventureWorksAIWorkspaceAPI.Application.User.GetUsers;

public sealed record GetUsersResponse(IReadOnlyList<UserDto> Users);
