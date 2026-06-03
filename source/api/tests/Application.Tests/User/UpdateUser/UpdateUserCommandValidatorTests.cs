using AdventureWorksAIWorkspace.Application.User.UpdateUser;

namespace AdventureWorksAIWorkspace.Application.Tests.User.UpdateUser;

public sealed class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenValid_ShouldBeValid()
    {
        var result = _validator.Validate(new UpdateUserCommand("id-1", "john", "john@example.com", "Admin"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenOnlyUserIdProvided_ShouldBeValid()
    {
        var result = _validator.Validate(new UpdateUserCommand("id-1"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenUserIdIsEmpty_ShouldBeInvalid(string? userId)
    {
        var result = _validator.Validate(new UpdateUserCommand(userId!));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.UserId));
    }

    [Fact]
    public void Validate_WhenUserNameExceedsMaxLength_ShouldBeInvalid()
    {
        var result = _validator.Validate(new UpdateUserCommand("id-1", UserName: new string('a', 257)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.UserName));
    }

    [Fact]
    public void Validate_WhenEmailIsInvalidFormat_ShouldBeInvalid()
    {
        var result = _validator.Validate(new UpdateUserCommand("id-1", Email: "not-an-email"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.Email));
    }

    [Fact]
    public void Validate_WhenEmailExceedsMaxLength_ShouldBeInvalid()
    {
        var result = _validator.Validate(new UpdateUserCommand("id-1", Email: new string('a', 252) + "@x.com"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.Email));
    }

    [Fact]
    public void Validate_WhenRoleExceedsMaxLength_ShouldBeInvalid()
    {
        var result = _validator.Validate(new UpdateUserCommand("id-1", Role: new string('a', 65)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.Role));
    }
}
