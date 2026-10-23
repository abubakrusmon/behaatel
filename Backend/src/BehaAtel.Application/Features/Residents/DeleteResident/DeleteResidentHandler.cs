using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Residents.DeleteResident;

public sealed class DeleteResidentHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IAuditService auditService)
    : IRequestHandler<DeleteResidentCommand>
{
    public async Task Handle(DeleteResidentCommand request, CancellationToken cancellationToken)
    {
        var residentWithBalance = await ResidentAccess.ScopeToCurrentUser(
                dbContext.Residents.Where(candidate => !candidate.IsDeleted), currentUser)
            .WithBalance(dbContext.Payments.AsNoTracking(), dbContext.Charges.AsNoTracking())
            .SingleOrDefaultAsync(candidate => candidate.Resident.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Resident), request.Id);
        var resident = residentWithBalance.Resident;

        if (resident.Status == ResidentStatus.Active)
        {
            throw new ConflictException("Нельзя удалить заселённого жильца.");
        }

        if (residentWithBalance.Balance < 0)
        {
            throw new ConflictException("Нельзя удалить жильца с непогашенным долгом.");
        }

        var user = await dbContext.Users
            .Include(candidate => candidate.RefreshTokens)
            .SingleOrDefaultAsync(candidate => candidate.Id == resident.UserId && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), resident.UserId);

        resident.MarkDeleted();
        user.Delete();
        foreach (var token in user.RefreshTokens.Where(token => token.IsActive))
        {
            token.Revoke();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("DeleteResident", nameof(Resident), resident.Id.ToString(),
            new { IsDeleted = false }, new { IsDeleted = true }, cancellationToken);
    }
}
