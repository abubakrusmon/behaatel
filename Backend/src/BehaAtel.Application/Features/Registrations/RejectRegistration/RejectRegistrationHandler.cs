using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Registrations.RejectRegistration;

public sealed class RejectRegistrationHandler(
    IApplicationDbContext db,
    IEmailSender emailSender,
    IAuditService audit) : IRequestHandler<RejectRegistrationCommand>
{
    public async Task Handle(RejectRegistrationCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(candidate => candidate.Id == request.UserId &&
            candidate.Role == Role.User && candidate.ApprovalStatus == ApprovalStatus.Pending, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);
        user.RejectRegistration();
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("RejectRegistration", nameof(User), user.Id.ToString(), null,
            new { user.Email, user.FullName }, cancellationToken);
        var mail = EmailTemplates.Rejected(request.Reason);
        await emailSender.SendAsync(user.Email, mail.Subject, mail.Body, cancellationToken);
    }
}
