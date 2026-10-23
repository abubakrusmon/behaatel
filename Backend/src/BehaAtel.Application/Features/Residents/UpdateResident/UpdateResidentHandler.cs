using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Residents.UpdateResident;

public sealed class UpdateResidentHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IAuditService auditService)
    : IRequestHandler<UpdateResidentCommand>
{
    public async Task Handle(UpdateResidentCommand request, CancellationToken cancellationToken)
    {
        var resident = await ResidentAccess.ScopeToCurrentUser(
                dbContext.Residents.Include(candidate => candidate.Occupancies)
                    .Where(candidate => !candidate.IsDeleted), currentUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Resident), request.Id);

        var passport = request.PassportNumber.Trim();
        var passportKey = passport.ToLower();
        if (await dbContext.Residents.AnyAsync(candidate =>
                candidate.Id != resident.Id && !candidate.IsDeleted &&
                candidate.PassportNumber.ToLower() == passportKey, cancellationToken))
        {
            throw new ConflictException("Жилец с таким номером паспорта уже существует.");
        }

        var user = await dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.Id == resident.UserId && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), resident.UserId);

        var oldValues = new
        {
            user.FullName,
            user.Phone,
            resident.PassportNumber,
            resident.BirthDate,
            resident.Organization
        };
        user.UpdateProfile(request.FullName, request.Phone);
        resident.Update(passport, request.BirthDate, request.Organization);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("UpdateResident", nameof(Resident), resident.Id.ToString(), oldValues,
            new
            {
                user.FullName,
                user.Phone,
                resident.PassportNumber,
                resident.BirthDate,
                resident.Organization
            }, cancellationToken);
    }
}
