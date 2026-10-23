using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using FluentAssertions;

namespace BehaAtel.Application.UnitTests.Features.Auth;

public sealed class EmailDeliveryPolicyTests
{
    private static readonly DateTime Now = new(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Registration_UnconfirmedUserCanReceiveAnotherVerificationCode()
    {
        var user = NewApplicant();

        EmailCodePolicy.CanSendVerification(user).Should().BeTrue();
        EmailCodePolicy.ShouldReopenRejectedRegistration(user).Should().BeFalse();
    }

    [Fact]
    public void Registration_RejectedUserIsReopenedAndCanReceiveVerificationCode()
    {
        var user = NewApplicant();
        user.ConfirmEmail();
        user.RejectRegistration();

        EmailCodePolicy.ShouldReopenRejectedRegistration(user).Should().BeTrue();
        user.ReopenRejectedRegistration("new-hash", "Updated Name", "123", "CD654321",
            new DateOnly(1991, 2, 3), "Updated organization", Now.AddMinutes(1));

        user.ApprovalStatus.Should().Be(ApprovalStatus.Pending);
        user.EmailConfirmed.Should().BeFalse();
        user.IsActive.Should().BeFalse();
        user.FullName.Should().Be("Updated Name");
        user.PasswordHash.Should().Be("new-hash");
        EmailCodePolicy.CanSendVerification(user).Should().BeTrue();
    }

    [Fact]
    public void Registration_ConfirmedApprovedUserDoesNotReceiveAnotherVerificationCode()
    {
        var user = User.Create("resident@example.com", "hash", "Resident");

        EmailCodePolicy.ShouldReopenRejectedRegistration(user).Should().BeFalse();
        EmailCodePolicy.CanSendVerification(user).Should().BeFalse();
    }

    [Theory]
    [InlineData(ApprovalStatus.Pending)]
    [InlineData(ApprovalStatus.Rejected)]
    public void ForgotPassword_ConfirmedPendingOrRejectedUserCanReceiveCode(ApprovalStatus status)
    {
        var user = NewApplicant();
        user.ConfirmEmail();
        if (status == ApprovalStatus.Rejected)
            user.RejectRegistration();

        EmailCodePolicy.CanRequestPasswordReset(user).Should().BeTrue();
    }

    [Fact]
    public void ForgotPassword_UnconfirmedUserDoesNotReceiveCode()
    {
        EmailCodePolicy.CanRequestPasswordReset(NewApplicant()).Should().BeFalse();
    }

    [Fact]
    public void RateLimit_EnforcesSixtySecondCooldown()
    {
        var recent = new[] { NewCode(Now.AddSeconds(-59)) };

        EmailCodePolicy.GetRateLimitReason(recent, Now).Should().Be(EmailCodeRateLimitReason.Cooldown);
        EmailCodePolicy.GetRateLimitReason([NewCode(Now.AddSeconds(-60))], Now).Should().BeNull();
    }

    [Fact]
    public void RateLimit_EnforcesFiveMessagesPerHourAcrossPurposes()
    {
        var recent = Enumerable.Range(0, 5)
            .Select(index => NewCode(Now.AddMinutes(-2 - index), (EmailCodePurpose)(index % 3)))
            .ToArray();

        EmailCodePolicy.GetRateLimitReason(recent, Now).Should().Be(EmailCodeRateLimitReason.HourlyLimit);
        EmailCodePolicy.GetRateLimitReason(recent.Take(4).ToArray(), Now).Should().BeNull();
    }

    private static User NewApplicant() =>
        User.CreateRegistration("applicant@example.com", "hash", "Applicant", null,
            "AB123456", new DateOnly(1990, 1, 1), null, Now);

    private static EmailCode NewCode(DateTime createdAt, EmailCodePurpose purpose = EmailCodePurpose.EmailVerification) =>
        EmailCode.Create(Guid.NewGuid(), purpose, "ABCD", createdAt.AddMinutes(15), createdAt);
}
