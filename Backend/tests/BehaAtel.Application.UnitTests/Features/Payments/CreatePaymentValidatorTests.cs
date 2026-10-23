using BehaAtel.Application.Features.Payments.CreatePayment;
using BehaAtel.Domain.Enums;
using FluentAssertions;

namespace BehaAtel.Application.UnitTests.Features.Payments;

public class CreatePaymentValidatorTests
{
    private readonly CreatePaymentValidator _validator = new();

    [Fact]
    public void Validate_ValidPayment_IsValid()
    {
        var result = _validator.Validate(new CreatePaymentCommand(
            Guid.NewGuid(),
            100m,
            PaymentMethod.Card,
            new DateTime(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc),
            null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NonPositiveAmount_IsInvalid()
    {
        var result = _validator.Validate(new CreatePaymentCommand(
            Guid.NewGuid(), 0m, PaymentMethod.Card));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreatePaymentCommand.Amount));
    }

    [Fact]
    public void Validate_NonUtcPaidAt_IsInvalid()
    {
        var result = _validator.Validate(new CreatePaymentCommand(
            Guid.NewGuid(),
            100m,
            PaymentMethod.Cash,
            DateTime.SpecifyKind(new DateTime(2026, 10, 19, 12, 0, 0), DateTimeKind.Local)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreatePaymentCommand.PaidAt));
    }
}
