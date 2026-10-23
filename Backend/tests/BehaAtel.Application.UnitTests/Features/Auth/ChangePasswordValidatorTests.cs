using BehaAtel.Application.Features.Auth.ChangePassword;
using FluentAssertions;

namespace BehaAtel.Application.UnitTests.Features.Auth;

public class ChangePasswordValidatorTests
{
    private readonly ChangePasswordValidator _validator = new();

    [Fact]
    public void Validate_StrongPassword_IsValid()
    {
        var result = _validator.Validate(new ChangePasswordCommand("old-password", "StrongPass1!"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Ab1!xyz")]
    [InlineData("abcdefgh1!")]
    [InlineData("ABCDEFGH1!")]
    [InlineData("Abcdefgh!")]
    [InlineData("Abcdefgh1")]
    public void Validate_WeakPassword_IsInvalid(string password)
    {
        var result = _validator.Validate(new ChangePasswordCommand("old-password", password));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_EmptyCurrentPassword_IsInvalid()
    {
        var result = _validator.Validate(new ChangePasswordCommand("", "StrongPass1!"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(ChangePasswordCommand.CurrentPassword));
    }
}
