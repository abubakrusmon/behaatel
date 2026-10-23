using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BehaAtel.Application.Features.Auth.Register;

public sealed class RegisterHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    IEmailCodeService codeService,
    IEmailSender emailSender,
    IDateTimeProvider clock,
    ILogger<RegisterHandler> logger) : IRequestHandler<RegisterCommand, AuthActionDto>
{
    public async Task<AuthActionDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(candidate =>
            candidate.Email.ToLower() == email, cancellationToken);
        if (user is null)
        {
            user = User.CreateRegistration(email, passwordHasher.Hash(request.Password), request.FullName,
                request.Phone, request.PassportNumber, request.BirthDate, request.Organization, clock.UtcNow);
            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
        }
        else if (EmailCodePolicy.ShouldReopenRejectedRegistration(user))
        {
            user.ReopenRejectedRegistration(passwordHasher.Hash(request.Password), request.FullName,
                request.Phone, request.PassportNumber, request.BirthDate, request.Organization, clock.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
        }
        else if (!EmailCodePolicy.CanSendVerification(user))
        {
            return new AuthActionDto("Если указанный адрес может быть зарегистрирован, на него отправлена инструкция.");
        }

        var result = await EmailCodePolicy.IssueAsync(db, codeService, emailSender, clock, user,
            EmailCodePurpose.EmailVerification, cancellationToken);
        if (result.RateLimitReason is { } reason)
            EmailCodePolicy.LogRateLimit(logger, user, EmailCodePurpose.EmailVerification, reason);

        return new AuthActionDto("Если указанный адрес может быть зарегистрирован, на него отправлена инструкция.");
    }
}
