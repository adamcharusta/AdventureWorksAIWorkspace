using AdventureWorksAIWorkspace.Application.User.CreateUser;

namespace AdventureWorksAIWorkspace.Application.Tests.User.CreateUser;

public sealed class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenValid_ShouldBeValid()
    {
        var result = _validator.Validate(new CreateUserCommand("john", "john@example.com", "User"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenRoleIsNull_ShouldBeValid()
    {
        var result = _validator.Validate(new CreateUserCommand("john", "john@example.com"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenUserNameIsEmpty_ShouldBeInvalid(string? userName)
    {
        var result = _validator.Validate(new CreateUserCommand(userName!, "john@example.com"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.UserName));
    }

    [Fact]
    public void Validate_WhenUserNameExceedsMaxLength_ShouldBeInvalid()
    {
        var result = _validator.Validate(new CreateUserCommand(new string('a', 257), "john@example.com"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.UserName));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenEmailIsEmpty_ShouldBeInvalid(string? email)
    {
        var result = _validator.Validate(new CreateUserCommand("john", email!));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Fact]
    public void Validate_WhenEmailIsInvalidFormat_ShouldBeInvalid()
    {
        var result = _validator.Validate(new CreateUserCommand("john", "not-an-email"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Fact]
    public void Validate_WhenEmailExceedsMaxLength_ShouldBeInvalid()
    {
        var result = _validator.Validate(new CreateUserCommand("john", new string('a', 252) + "@x.com"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Fact]
    public void Validate_WhenRoleExceedsMaxLength_ShouldBeInvalid()
    {
        var result = _validator.Validate(new CreateUserCommand("john", "john@example.com", new string('a', 65)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Role));
    }
}
