using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Auth.Common;

public static class LoginAccessPolicy
{
    public static void EnsureEligible(User user)
    {
        if (!user.EmailConfirmed) throw new ForbiddenException("Подтвердите почту.");
        if (user.ApprovalStatus == ApprovalStatus.Pending)
            throw new ForbiddenException("Заявка ожидает одобрения.");
        if (user.ApprovalStatus == ApprovalStatus.Rejected)
            throw new ForbiddenException("Заявка отклонена.");
    }
}
