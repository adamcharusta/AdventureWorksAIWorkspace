using AdventureWorksAIWorkspaceAPI.Api.Authentication;
using AdventureWorksAIWorkspaceAPI.Application.User.CreateUser;
using AdventureWorksAIWorkspaceAPI.Application.User.DeleteUser;
using AdventureWorksAIWorkspaceAPI.Application.User.GetAssignableRoles;
using AdventureWorksAIWorkspaceAPI.Application.User.GetUsers;
using AdventureWorksAIWorkspaceAPI.Application.User.UpdateUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace AdventureWorksAIWorkspaceAPI.Api.Endpoints;

public static class UserEndpoints
{
    [WolverineGet(
        "/api/users/roles",
        Name = "GetAssignableRoles",
        OperationId = "GetAssignableRoles",
        Summary = "Returns assignable user roles.",
        Description = "Returns all roles that can be assigned to users. Only accessible by Admin users.")]
    [Tags("Users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<GetAssignableRolesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<Ok<GetAssignableRolesResponse>> GetAssignableRoles(
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        GetAssignableRolesResponse response =
            await messageBus.InvokeAsync<GetAssignableRolesResponse>(new GetAssignableRolesQuery(), cancellationToken);
        return TypedResults.Ok(response);
    }

    [WolverineGet(
        "/api/users",
        Name = "GetUsers",
        OperationId = "GetUsers",
        Summary = "Returns a list of all users.",
        Description =
            "Returns all registered users with their ID, user name, email, and assigned role. Only accessible by Admin users.")]
    [Tags("Users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<GetUsersResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<Ok<GetUsersResponse>> GetUsers(
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        GetUsersResponse response =
            await messageBus.InvokeAsync<GetUsersResponse>(new GetUsersQuery(), cancellationToken);
        return TypedResults.Ok(response);
    }

    [WolverinePost(
        "/api/users",
        Name = "CreateUser",
        OperationId = "CreateUser",
        Summary = "Creates a new user account.",
        Description =
            "Creates a new user with the specified user name, email, and optional role. The account is created with the template password and marked as requiring a first-login password change. Only accessible by Admin users.")]
    [Tags("Users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<CreateUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<Ok<CreateUserResponse>> CreateUser(
        [FromBody] CreateUserCommand command,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        CreateUserResponse response = await messageBus.InvokeAsync<CreateUserResponse>(command, cancellationToken);
        return TypedResults.Ok(response);
    }

    [WolverinePut(
        "/api/users/{userId}",
        Name = "UpdateUser",
        OperationId = "UpdateUser",
        Summary = "Updates an existing user account.",
        Description =
            "Updates user name, email, and/or role. When ResetPassword is true, the password is reset to the template password and the account is marked as requiring a first-login password change. Only accessible by Admin users.")]
    [Tags("Users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<UpdateUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<Ok<UpdateUserResponse>> UpdateUser(
        string userId,
        [FromBody] UpdateUserCommand command,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        UpdateUserCommand commandWithId = command with { UserId = userId };
        UpdateUserResponse response =
            await messageBus.InvokeAsync<UpdateUserResponse>(commandWithId, cancellationToken);
        return TypedResults.Ok(response);
    }

    [WolverineDelete(
        "/api/users/{userId}",
        Name = "DeleteUser",
        OperationId = "DeleteUser",
        Summary = "Deletes an existing user account.",
        Description =
            "Deletes a user account by ID. Only accessible by Admin users. Admin users cannot delete their own account.")]
    [Tags("Users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<NoContent> DeleteUser(
        string userId,
        HttpContext httpContext,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        string? currentUserId = httpContext.GetCurrentUserId();

        await messageBus.InvokeAsync(new DeleteUserCommand(userId, currentUserId), cancellationToken);
        return TypedResults.NoContent();
    }
}
