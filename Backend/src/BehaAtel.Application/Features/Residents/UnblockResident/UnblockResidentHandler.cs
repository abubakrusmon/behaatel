using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Residents.UnblockResident;

public sealed class UnblockResidentHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IAuditService auditService)
    : IRequestHandler<UnblockResidentCommand>
{
    public async Task Handle(UnblockResidentCommand request, CancellationToken cancellationToken)
    {
        var resident = await ResidentAccess.ScopeToCurrentUser(
                dbContext.Residents.Where(candidate => !candidate.IsDeleted), currentUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.ResidentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Resident), request.ResidentId);
        var user = await dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.Id == resident.UserId && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), resident.UserId);

        var wasActive = user.IsActive;
        user.Unblock();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("UnblockResident", nameof(Resident), resident.Id.ToString(),
            new { IsActive = wasActive }, new { user.IsActive }, cancellationToken);
    }
}
