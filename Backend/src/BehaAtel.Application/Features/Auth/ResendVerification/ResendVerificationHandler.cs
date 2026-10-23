using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Application.Features.Auth.Register;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BehaAtel.Application.Features.Auth.ResendVerification;

public sealed class ResendVerificationHandler(
    IApplicationDbContext db,
    IEmailCodeService codeService,
    IEmailSender emailSender,
    IDateTimeProvider clock,
    ILogger<ResendVerificationHandler> logger) : IRequestHandler<ResendVerificationCommand, AuthActionDto>
{
    public async Task<AuthActionDto> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(candidate =>
            candidate.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);
        if (user is not null && EmailCodePolicy.CanSendVerification(user))
        {
            var result = await EmailCodePolicy.IssueAsync(db, codeService, emailSender, clock, user,
                EmailCodePurpose.EmailVerification, cancellationToken);
            if (result.RateLimitReason is { } reason)
                EmailCodePolicy.LogRateLimit(logger, user, EmailCodePurpose.EmailVerification, reason);
        }

        return new AuthActionDto("Если для этого адреса доступна отправка, письмо будет отправлено.");
    }
}
