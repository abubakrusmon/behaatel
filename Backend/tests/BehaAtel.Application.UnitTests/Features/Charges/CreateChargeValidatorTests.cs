using BehaAtel.Application.Features.Charges.CreateCharge;
using BehaAtel.Domain.Enums;
using FluentAssertions;

namespace BehaAtel.Application.UnitTests.Features.Charges;

public class CreateChargeValidatorTests
{
    private readonly CreateChargeValidator _validator = new();

    [Fact]
    public void Validate_ValidCharge_IsValid()
    {
        var result = _validator.Validate(new CreateChargeCommand(
            Guid.NewGuid(),
            new DateOnly(2026, 10, 1),
            ChargeType.Rent,
            100m,
            new DateOnly(2026, 10, 10),
            null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NonPositiveAmount_IsInvalid()
    {
        var result = _validator.Validate(new CreateChargeCommand(
            Guid.NewGuid(),
            new DateOnly(2026, 10, 1),
            ChargeType.Rent,
            0m,
            new DateOnly(2026, 10, 10),
            null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateChargeCommand.Amount));
    }

    [Fact]
    public void Validate_UnknownChargeType_IsInvalid()
    {
        var result = _validator.Validate(new CreateChargeCommand(
            Guid.NewGuid(),
            new DateOnly(2026, 10, 1),
            (ChargeType)999,
            100m,
            new DateOnly(2026, 10, 10),
            null));

        result.IsValid.Should().BeFalse();
    }
}
