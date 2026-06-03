using AdventureWorksAIWorkspace.Application.Auth.Refresh;

namespace AdventureWorksAIWorkspace.Application.Tests.Auth.Refresh;

public sealed class RefreshCommandValidatorTests
{
    private readonly RefreshCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenValid_ShouldBeValid()
    {
        var result = _validator.Validate(new RefreshCommand("some-refresh-token"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenRefreshTokenIsEmpty_ShouldBeInvalid(string? refreshToken)
    {
        var result = _validator.Validate(new RefreshCommand(refreshToken!));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RefreshCommand.RefreshToken));
    }
}
