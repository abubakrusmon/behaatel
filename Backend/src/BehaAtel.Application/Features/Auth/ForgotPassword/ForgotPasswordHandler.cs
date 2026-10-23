using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Application.Features.Auth.Register;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BehaAtel.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordHandler(
    IApplicationDbContext db,
    IEmailCodeService codeService,
    IEmailSender emailSender,
    IDateTimeProvider clock,
    ILogger<ForgotPasswordHandler> logger) : IRequestHandler<ForgotPasswordCommand, AuthActionDto>
{
    public async Task<AuthActionDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(candidate =>
            candidate.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);
        if (user is not null && EmailCodePolicy.CanRequestPasswordReset(user))
        {
            var result = await EmailCodePolicy.IssueAsync(db, codeService, emailSender, clock, user,
                EmailCodePurpose.PasswordReset, cancellationToken);
            if (result.RateLimitReason is { } reason)
                EmailCodePolicy.LogRateLimit(logger, user, EmailCodePurpose.PasswordReset, reason);
        }

        return new AuthActionDto("Если аккаунт с таким адресом существует, письмо с инструкцией отправлено.");
    }
}
