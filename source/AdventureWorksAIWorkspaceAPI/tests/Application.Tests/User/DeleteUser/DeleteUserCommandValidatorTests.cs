using AdventureWorksAIWorkspaceAPI.Application.User.DeleteUser;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.User.DeleteUser;

public sealed class DeleteUserCommandValidatorTests
{
    private readonly DeleteUserCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenValid_ShouldBeValid()
    {
        var result = _validator.Validate(new DeleteUserCommand("target-id", "admin-id"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenUserIdIsEmpty_ShouldBeInvalid(string? userId)
    {
        var result = _validator.Validate(new DeleteUserCommand(userId!, "admin-id"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteUserCommand.UserId));
    }
}
