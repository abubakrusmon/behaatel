using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Register;
using BehaAtel.Application.Features.Auth.ResetPassword;
using FluentAssertions;
using Moq;

namespace BehaAtel.Application.UnitTests.Features.Auth;

public sealed class EmailAuthValidatorTests
{
    private readonly DateTime _now = new(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
    private readonly RegisterValidator _register;
    private readonly ResetPasswordValidator _reset = new();

    public EmailAuthValidatorTests()
    {
        var clock = new Mock<IDateTimeProvider>();
        clock.Setup(value => value.UtcNow).Returns(_now);
        _register = new RegisterValidator(clock.Object);
    }

    [Fact]
    public void RegisterValidator_AcceptsValidApplicant()
    {
        var result = _register.Validate(new RegisterCommand("resident@example.com", "Resident Name",
            null, "StrongPass1!", "AB123456", new DateOnly(1990, 1, 1), null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RegisterValidator_RejectsWeakPassword()
    {
        var result = _register.Validate(new RegisterCommand("resident@example.com", "Resident Name",
            null, "weak", "AB123456", new DateOnly(1990, 1, 1), null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public void ResetPasswordValidator_RequiresStrongPasswordAndSixDigitCode()
    {
        var valid = _reset.Validate(new ResetPasswordCommand("resident@example.com", "123456", "StrongPass1!"));
        var invalid = _reset.Validate(new ResetPasswordCommand("resident@example.com", "12345", "weak"));

        valid.IsValid.Should().BeTrue();
        invalid.IsValid.Should().BeFalse();
        invalid.Errors.Should().Contain(error => error.PropertyName == nameof(ResetPasswordCommand.NewPassword));
        invalid.Errors.Should().Contain(error => error.PropertyName == nameof(ResetPasswordCommand.Code));
    }
}
