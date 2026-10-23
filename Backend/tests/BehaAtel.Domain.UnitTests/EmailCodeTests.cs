using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using FluentAssertions;

namespace BehaAtel.Domain.UnitTests;

public sealed class EmailCodeTests
{
    private static readonly DateTime Now = new(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void TryVerify_RejectsExpiredCode()
    {
        var code = EmailCode.Create(Guid.NewGuid(), EmailCodePurpose.EmailVerification, "ABCD",
            Now.AddMinutes(-1), Now.AddMinutes(-16));

        code.TryVerify("ABCD", Now).Should().BeFalse();
        code.Attempts.Should().Be(0);
    }

    [Fact]
    public void TryVerify_ExpiresAfterFiveIncorrectAttempts()
    {
        var code = EmailCode.Create(Guid.NewGuid(), EmailCodePurpose.EmailVerification, "AAAA",
            Now.AddMinutes(15), Now);

        for (var attempt = 0; attempt < EmailCode.MaximumAttempts; attempt++)
            code.TryVerify("BBBB", Now).Should().BeFalse();

        code.Attempts.Should().Be(EmailCode.MaximumAttempts);
        code.TryVerify("AAAA", Now).Should().BeFalse();
        code.Attempts.Should().Be(EmailCode.MaximumAttempts);
    }

    [Fact]
    public void TryVerify_ConsumesMatchingCode()
    {
        var code = EmailCode.Create(Guid.NewGuid(), EmailCodePurpose.LoginTwoFactor, "AAAA",
            Now.AddMinutes(15), Now);

        code.TryVerify("AAAA", Now).Should().BeTrue();
        code.ConsumedAt.Should().Be(Now);
        code.TryVerify("AAAA", Now).Should().BeFalse();
    }
}
