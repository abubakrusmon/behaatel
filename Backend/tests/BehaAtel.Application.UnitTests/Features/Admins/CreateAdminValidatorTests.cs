using BehaAtel.Application.Features.Admins.CreateAdmin;
using BehaAtel.Domain.Enums;
using FluentAssertions;

namespace BehaAtel.Application.UnitTests.Features.Admins;

public class CreateAdminValidatorTests
{
    private readonly CreateAdminValidator _validator = new();

    [Fact]
    public void Validate_ValidAdmin_IsValid()
    {
        var result = _validator.Validate(new CreateAdminCommand(
            "admin@example.com", "Admin User", null, "StrongPass1!", [Permission.ResidentsView]));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("not-an-email", "StrongPass1!")]
    [InlineData("admin@example.com", "weak")]
    public void Validate_InvalidEmailOrPassword_IsInvalid(string email, string password)
    {
        var result = _validator.Validate(new CreateAdminCommand(email, "Admin User", null, password, []));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_UnknownPermission_IsInvalid()
    {
        var result = _validator.Validate(new CreateAdminCommand(
            "admin@example.com", "Admin User", null, "StrongPass1!", [(Permission)999]));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName.StartsWith(nameof(CreateAdminCommand.Permissions), StringComparison.Ordinal));
    }
}
