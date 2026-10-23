using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Application.Features.Auth.Register;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Auth.VerifyEmail;

public sealed class VerifyEmailHandler(
    IApplicationDbContext db,
    IEmailCodeService codeService,
    IDateTimeProvider clock) : IRequestHandler<VerifyEmailCommand, AuthActionDto>
{
    public async Task<AuthActionDto> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(candidate =>
            candidate.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);
        var emailCode = user is null ? null : await db.EmailCodes
            .Where(code => code.UserId == user.Id && code.Purpose == EmailCodePurpose.EmailVerification)
            .OrderByDescending(code => code.CreatedAt).FirstOrDefaultAsync(cancellationToken);

        if (emailCode is null || !codeService.VerifyCode(emailCode, request.Code, clock.UtcNow))
        {
            if (emailCode is not null) await db.SaveChangesAsync(cancellationToken);
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["code"] = ["Неверный или просроченный код."]
            });
        }

        user!.ConfirmEmail();
        await db.SaveChangesAsync(cancellationToken);
        return new AuthActionDto("Почта подтверждена. Заявка отправлена администратору, дождитесь одобрения.");
    }
}
