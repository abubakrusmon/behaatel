using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using FluentAssertions;

namespace BehaAtel.Application.UnitTests.Features.Auth;

public sealed class LoginAccessPolicyTests
{
    private static readonly DateTime Now = new(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void EnsureEligible_RejectsUnconfirmedEmail()
    {
        var user = NewApplicant();
        Action act = () => LoginAccessPolicy.EnsureEligible(user);
        act.Should().Throw<ForbiddenException>()
            .WithMessage("Подтвердите почту.");
    }

    [Fact]
    public void EnsureEligible_RejectsPendingApplicant()
    {
        var user = NewApplicant();
        user.ConfirmEmail();
        Action act = () => LoginAccessPolicy.EnsureEligible(user);
        act.Should().Throw<ForbiddenException>()
            .WithMessage("Заявка ожидает одобрения.");
    }

    [Fact]
    public void EnsureEligible_RejectsRejectedApplicant()
    {
        var user = NewApplicant();
        user.ConfirmEmail();
        user.RejectRegistration();
        Action act = () => LoginAccessPolicy.EnsureEligible(user);
        act.Should().Throw<ForbiddenException>()
            .WithMessage("Заявка отклонена.");
    }

    private static User NewApplicant() =>
        User.CreateRegistration("applicant@example.com", "hash", "Applicant", null,
            "AB123456", new DateOnly(1990, 1, 1), null, Now);
}
