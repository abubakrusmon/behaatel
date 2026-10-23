using BehaAtel.Application.Features.Settings.Common;
using BehaAtel.Application.Features.Settings.UpdateSetting;
using FluentAssertions;

namespace BehaAtel.Application.UnitTests.Features.Settings;

public class UpdateSettingValidatorTests
{
    private readonly UpdateSettingValidator _validator = new();

    [Theory]
    [InlineData(SettingKeys.BillingDueDay, "1")]
    [InlineData(SettingKeys.BillingDueDay, "28")]
    [InlineData(SettingKeys.BillingCurrency, "TJS")]
    [InlineData(SettingKeys.BillingCurrency, "usd")]
    [InlineData(SettingKeys.BillingPenaltyPercent, "0")]
    [InlineData(SettingKeys.BillingPenaltyPercent, "100")]
    [InlineData(SettingKeys.BillingPenaltyPercent, "12.5")]
    public void Validate_ValidSettingValue_IsValid(string key, string value)
    {
        var result = _validator.Validate(new UpdateSettingCommand(key, value));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(SettingKeys.BillingDueDay, "0")]
    [InlineData(SettingKeys.BillingDueDay, "29")]
    [InlineData(SettingKeys.BillingDueDay, "1.5")]
    [InlineData(SettingKeys.BillingCurrency, "TJ")]
    [InlineData(SettingKeys.BillingCurrency, "US12")]
    [InlineData(SettingKeys.BillingPenaltyPercent, "-1")]
    [InlineData(SettingKeys.BillingPenaltyPercent, "100.1")]
    [InlineData(SettingKeys.BillingPenaltyPercent, "abc")]
    public void Validate_InvalidSettingValue_IsInvalid(string key, string value)
    {
        var result = _validator.Validate(new UpdateSettingCommand(key, value));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UpdateSettingCommand.Value));
    }

    [Fact]
    public void Validate_UnknownKey_IsInvalid()
    {
        var result = _validator.Validate(new UpdateSettingCommand("Billing.Unknown", "1"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UpdateSettingCommand.Key));
    }
}
