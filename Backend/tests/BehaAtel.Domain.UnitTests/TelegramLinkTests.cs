using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using FluentAssertions;

namespace BehaAtel.Domain.UnitTests;

public sealed class TelegramLinkTests
{
    private static readonly DateTime Now = new(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void AdminCreatedResidentWithoutPhone_IsExplicitlyExempt()
    {
        var resident = User.CreateAdminResident("resident@example.test", "hash", "Resident", null);

        resident.TelegramLinkExempt.Should().BeTrue();
        resident.RequiresTelegramLink.Should().BeFalse();
    }

    [Fact]
    public void SelfRegisteredAndAdminAccounts_RequireTelegramEvenWithoutPhone()
    {
        var resident = User.CreateRegistration("resident@example.test", "hash", "Resident", null,
            "passport", new DateOnly(1990, 1, 1), null, Now);
        var admin = User.Create("admin@example.test", "hash", "Admin", Role.Admin);

        resident.RequiresTelegramLink.Should().BeTrue();
        admin.RequiresTelegramLink.Should().BeTrue();
    }

    [Fact]
    public void ExemptAdminCreatedResident_RequiresLinkIfPhoneIsLaterAdded()
    {
        var resident = User.CreateAdminResident("resident@example.test", "hash", "Resident", null);

        resident.UpdateProfile("Resident", "+123456789");

        resident.TelegramLinkExempt.Should().BeTrue();
        resident.RequiresTelegramLink.Should().BeTrue();
    }

    [Fact]
    public void LinkTelegram_CannotRebindAnAccount()
    {
        var user = User.Create("resident@example.test", "hash", "Resident");
        user.LinkTelegram("10001");

        var act = () => user.LinkTelegram("10002");

        act.Should().Throw<BehaAtel.Domain.Exceptions.DomainException>();
        user.TelegramChatId.Should().Be("10001");
    }

    [Fact]
    public void TelegramLinkAttempt_ExpiresAndCanOnlyBeConsumedOnce()
    {
        var expired = TelegramLinkAttempt.Create(Guid.NewGuid(), "start-hash", "poll-hash", Now);
        var expireAct = () => expired.MarkBound(Now);
        expireAct.Should().Throw<InvalidOperationException>();

        var attempt = TelegramLinkAttempt.Create(Guid.NewGuid(), "start-hash", "poll-hash", Now.AddMinutes(10));
        attempt.MarkBound(Now);
        attempt.MarkConsumed(Now.AddSeconds(1));

        var consumeAgain = () => attempt.MarkConsumed(Now.AddSeconds(2));
        consumeAgain.Should().Throw<InvalidOperationException>();
    }
}
