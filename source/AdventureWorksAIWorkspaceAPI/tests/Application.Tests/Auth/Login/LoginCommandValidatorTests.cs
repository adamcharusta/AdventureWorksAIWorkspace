using AdventureWorksAIWorkspaceAPI.Application.Auth.Login;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.Auth.Login;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenValid_ShouldBeValid()
    {
        var result = _validator.Validate(new LoginCommand("admin", "password"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData(null, "password")]
    public void Validate_WhenIdentifierIsEmpty_ShouldBeInvalid(string? identifier, string password)
    {
        var result = _validator.Validate(new LoginCommand(identifier!, password));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Identifier));
    }

    [Theory]
    [InlineData("admin", "")]
    [InlineData("admin", null)]
    public void Validate_WhenPasswordIsEmpty_ShouldBeInvalid(string identifier, string? password)
    {
        var result = _validator.Validate(new LoginCommand(identifier, password!));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }

    [Fact]
    public void Validate_WhenIdentifierExceedsMaxLength_ShouldBeInvalid()
    {
        var result = _validator.Validate(new LoginCommand(new string('a', 257), "password"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Identifier));
    }
}
