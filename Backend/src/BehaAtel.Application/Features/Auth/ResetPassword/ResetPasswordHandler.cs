using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Application.Features.Auth.Register;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IEmailCodeService codeService,
    IDateTimeProvider clock) : IRequestHandler<ResetPasswordCommand, AuthActionDto>
{
    public async Task<AuthActionDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(candidate =>
            candidate.Email.ToLower() == request.Email.Trim().ToLower(), cancellationToken);
        var emailCode = user is null ? null : await db.EmailCodes
            .Where(code => code.UserId == user.Id && code.Purpose == EmailCodePurpose.PasswordReset)
            .OrderByDescending(code => code.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        if (emailCode is null || !codeService.VerifyCode(emailCode, request.Code, clock.UtcNow))
        {
            if (emailCode is not null) await db.SaveChangesAsync(cancellationToken);
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["code"] = ["Неверный или просроченный код."]
            });
        }

        user!.SetPassword(passwordHasher.Hash(request.NewPassword), false);
        var refreshTokens = await db.RefreshTokens
            .Where(token => token.UserId == user.Id && token.RevokedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var token in refreshTokens) token.Revoke();
        await db.SaveChangesAsync(cancellationToken);
        return new AuthActionDto("Пароль успешно изменён.");
    }
}
