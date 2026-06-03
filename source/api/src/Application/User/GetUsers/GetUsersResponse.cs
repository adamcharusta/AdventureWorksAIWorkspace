using AdventureWorksAIWorkspace.Application.Common.Dtos;
using AdventureWorksAIWorkspace.Application.Common.Dtos.User;

namespace AdventureWorksAIWorkspace.Application.User.GetUsers;

public sealed record GetUsersResponse(IReadOnlyList<UserDto> Users);
