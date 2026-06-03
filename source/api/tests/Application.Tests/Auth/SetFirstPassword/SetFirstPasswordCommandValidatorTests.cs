using AdventureWorksAIWorkspace.Application.Auth.SetFirstPassword;

namespace AdventureWorksAIWorkspace.Application.Tests.Auth.SetFirstPassword;

public sealed class SetFirstPasswordCommandValidatorTests
{
    private readonly SetFirstPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenValid_ShouldBeValid()
    {
        var result = _validator.Validate(new SetFirstPasswordCommand("admin", "NewPass1!", "NewPass1!"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "NewPass1!", "NewPass1!")]
    [InlineData(null, "NewPass1!", "NewPass1!")]
    public void Validate_WhenIdentifierIsEmpty_ShouldBeInvalid(string? identifier, string confirm, string password)
    {
        var result = _validator.Validate(new SetFirstPasswordCommand(identifier!, confirm, password));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetFirstPasswordCommand.Identifier));
    }

    [Fact]
    public void Validate_WhenIdentifierExceedsMaxLength_ShouldBeInvalid()
    {
        var result = _validator.Validate(new SetFirstPasswordCommand(new string('a', 257), "NewPass1!", "NewPass1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetFirstPasswordCommand.Identifier));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenNewPasswordIsEmpty_ShouldBeInvalid(string? newPassword)
    {
        var result = _validator.Validate(new SetFirstPasswordCommand("admin", "NewPass1!", newPassword!));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetFirstPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_WhenNewPasswordTooShort_ShouldBeInvalid()
    {
        var result = _validator.Validate(new SetFirstPasswordCommand("admin", "Sh1!", "Sh1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetFirstPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_WhenNewPasswordExceedsMaxLength_ShouldBeInvalid()
    {
        var longPassword = new string('a', 129) + "A1!";
        var result = _validator.Validate(new SetFirstPasswordCommand("admin", longPassword, longPassword));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetFirstPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_WhenNewPasswordMissingLowercase_ShouldBeInvalid()
    {
        var result = _validator.Validate(new SetFirstPasswordCommand("admin", "NEWPASS1!", "NEWPASS1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetFirstPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_WhenNewPasswordMissingUppercase_ShouldBeInvalid()
    {
        var result = _validator.Validate(new SetFirstPasswordCommand("admin", "newpass1!", "newpass1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetFirstPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_WhenNewPasswordMissingDigit_ShouldBeInvalid()
    {
        var result = _validator.Validate(new SetFirstPasswordCommand("admin", "NewPasss!", "NewPasss!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetFirstPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_WhenNewPasswordMissingSpecialChar_ShouldBeInvalid()
    {
        var result = _validator.Validate(new SetFirstPasswordCommand("admin", "NewPass11", "NewPass11"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetFirstPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_WhenConfirmDoesNotMatchNewPassword_ShouldBeInvalid()
    {
        var result = _validator.Validate(new SetFirstPasswordCommand("admin", "Different1!", "NewPass1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetFirstPasswordCommand.ConfirmNewPassword));
    }
}
