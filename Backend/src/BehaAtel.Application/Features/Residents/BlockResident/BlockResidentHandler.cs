using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Residents.BlockResident;

public sealed class BlockResidentHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IAuditService auditService)
    : IRequestHandler<BlockResidentCommand>
{
    public async Task Handle(BlockResidentCommand request, CancellationToken cancellationToken)
    {
        var resident = await ResidentAccess.ScopeToCurrentUser(
                dbContext.Residents.Where(candidate => !candidate.IsDeleted), currentUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.ResidentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Resident), request.ResidentId);
        var user = await dbContext.Users
            .Include(candidate => candidate.RefreshTokens)
            .SingleOrDefaultAsync(candidate => candidate.Id == resident.UserId && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), resident.UserId);

        var wasActive = user.IsActive;
        user.Block();
        foreach (var token in user.RefreshTokens.Where(token => token.IsActive))
        {
            token.Revoke();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("BlockResident", nameof(Resident), resident.Id.ToString(),
            new { IsActive = wasActive }, new { user.IsActive }, cancellationToken);
    }
}
