using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Registrations.ApproveRegistration;

public sealed class ApproveRegistrationHandler(
    IApplicationDbContext db,
    IEmailSender emailSender,
    IAuditService audit) : IRequestHandler<ApproveRegistrationCommand>
{
    public async Task Handle(ApproveRegistrationCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(candidate => candidate.Id == request.UserId &&
            candidate.Role == Role.User && candidate.ApprovalStatus == ApprovalStatus.Pending, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);
        if (!user.EmailConfirmed) throw new ConflictException("Пользователь ещё не подтвердил адрес электронной почты.");
        user.ApproveRegistration();
        var profile = user.TakeRegistrationProfile();
        if (await db.Residents.AnyAsync(resident =>
                resident.PassportNumber.ToLower() == profile.PassportNumber.ToLower(), cancellationToken))
            throw new ConflictException("Жилец с таким номером паспорта уже существует.");
        db.Residents.Add(Resident.Create(user.Id, profile.PassportNumber, profile.BirthDate, profile.Organization));
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("ApproveRegistration", nameof(User), user.Id.ToString(), null,
            new { user.Email, user.FullName }, cancellationToken);
        var mail = EmailTemplates.Approved();
        await emailSender.SendAsync(user.Email, mail.Subject, mail.Body, cancellationToken);
    }
}
