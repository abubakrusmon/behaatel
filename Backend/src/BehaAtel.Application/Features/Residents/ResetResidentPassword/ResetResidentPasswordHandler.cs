using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Residents.ResetResidentPassword;

public sealed class ResetResidentPasswordHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    IAuditService auditService)
    : IRequestHandler<ResetResidentPasswordCommand>
{
    public async Task Handle(ResetResidentPasswordCommand request, CancellationToken cancellationToken)
    {
        var resident = await ResidentAccess.ScopeToCurrentUser(
                dbContext.Residents.Where(candidate => !candidate.IsDeleted), currentUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.ResidentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Resident), request.ResidentId);
        var user = await dbContext.Users
            .Include(candidate => candidate.RefreshTokens)
            .SingleOrDefaultAsync(candidate => candidate.Id == resident.UserId && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), resident.UserId);

        user.SetPassword(passwordHasher.Hash(request.NewPassword), mustChange: true);
        foreach (var token in user.RefreshTokens.Where(token => token.IsActive))
        {
            token.Revoke();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("ResetResidentPassword", nameof(Resident), resident.Id.ToString(),
            null, new { MustChangePassword = true }, cancellationToken);
    }
}
