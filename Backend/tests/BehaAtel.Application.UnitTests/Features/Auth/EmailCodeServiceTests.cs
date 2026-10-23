using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using BehaAtel.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace BehaAtel.Application.UnitTests.Features.Auth;

public sealed class EmailCodeServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
    private readonly InfrastructureEmailCodeService _service = new(new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["Auth:EmailCodePepper"] = "unit-test-pepper" })
        .Build());

    [Fact]
    public void GenerateCode_ReturnsSixRandomDigits()
    {
        _service.GenerateCode().Should().MatchRegex("^\\d{6}$");
    }

    [Fact]
    public void HashCode_IsNotPlaintextAndVerifiesCode()
    {
        const string raw = "123456";
        var hash = _service.HashCode(raw);
        var entity = EmailCode.Create(Guid.NewGuid(), EmailCodePurpose.EmailVerification, hash, Now.AddMinutes(15), Now);

        hash.Should().NotContain(raw);
        _service.VerifyCode(entity, raw, Now).Should().BeTrue();
    }

    [Fact]
    public void VerifyCode_TracksFiveFailedAttempts()
    {
        var entity = EmailCode.Create(Guid.NewGuid(), EmailCodePurpose.PasswordReset,
            _service.HashCode("123456"), Now.AddMinutes(15), Now);

        for (var attempt = 0; attempt < EmailCode.MaximumAttempts; attempt++)
            _service.VerifyCode(entity, "000000", Now).Should().BeFalse();

        entity.Attempts.Should().Be(5);
        _service.VerifyCode(entity, "123456", Now).Should().BeFalse();
    }
}
