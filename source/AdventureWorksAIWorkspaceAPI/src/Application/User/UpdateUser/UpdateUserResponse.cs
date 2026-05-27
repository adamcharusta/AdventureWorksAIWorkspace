namespace AdventureWorksAIWorkspaceAPI.Application.User.UpdateUser;

public sealed record UpdateUserResponse(string UserId, string UserName, string Email, string Role);
