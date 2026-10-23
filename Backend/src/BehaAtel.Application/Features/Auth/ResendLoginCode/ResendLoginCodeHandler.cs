using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Application.Features.Auth.Register;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Auth.ResendLoginCode;

public sealed class ResendLoginCodeHandler(
    IApplicationDbContext db,
    IEmailCodeService codeService,
    IEmailSender emailSender,
    IDateTimeProvider clock) : IRequestHandler<ResendLoginCodeCommand, AuthActionDto>
{
    public async Task<AuthActionDto> Handle(ResendLoginCodeCommand request, CancellationToken cancellationToken)
    {
        var code = await db.EmailCodes.SingleOrDefaultAsync(candidate =>
            candidate.Id == request.ChallengeId && candidate.Purpose == EmailCodePurpose.LoginTwoFactor,
            cancellationToken);
        var user = code is null || !code.IsUsable(clock.UtcNow) ? null : await db.Users.SingleOrDefaultAsync(candidate =>
            candidate.Id == code.UserId && candidate.IsActive && candidate.EmailConfirmed &&
            candidate.ApprovalStatus == ApprovalStatus.Approved,
            cancellationToken);
        if (user is not null)
        {
            var result = await EmailCodePolicy.IssueAsync(db, codeService, emailSender, clock, user,
                EmailCodePurpose.LoginTwoFactor, cancellationToken);
            if (result.RateLimitReason is not null)
                return new AuthActionDto("Слишком часто запрашивали код. Попробуйте позже.");
        }
        return new AuthActionDto("Если код доступен для повторной отправки, письмо будет отправлено.");
    }
}
