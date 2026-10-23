using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Residents.CreateResident;

public sealed class CreateResidentHandler(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IAuditService auditService)
    : IRequestHandler<CreateResidentCommand, ResidentListItemDto>
{
    public async Task<ResidentListItemDto> Handle(CreateResidentCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var emailKey = email.ToLower();
        if (await dbContext.Users.AnyAsync(user => user.Email.ToLower() == emailKey, cancellationToken))
        {
            throw new ConflictException("Пользователь с таким email уже существует.");
        }

        var passport = request.PassportNumber.Trim();
        var passportKey = passport.ToLower();
        if (await dbContext.Residents.AnyAsync(resident =>
                !resident.IsDeleted && resident.PassportNumber.ToLower() == passportKey, cancellationToken))
        {
            throw new ConflictException("Жилец с таким номером паспорта уже существует.");
        }

        var residentUser = User.Create(email, passwordHasher.Hash(request.Password), request.FullName,
            Role.User, request.Phone, mustChangePassword: true);
        var resident = Resident.Create(residentUser.Id, passport, request.BirthDate, request.Organization);
        string? roomNumber = null;
        string? buildingName = null;

        if (request.RoomId is { } roomId && request.CheckInDate is { } checkInDate)
        {
            var room = await dbContext.Rooms
                .Include(candidate => candidate.Occupancies)
                .SingleOrDefaultAsync(candidate => candidate.Id == roomId && !candidate.IsDeleted, cancellationToken)
                ?? throw new NotFoundException(nameof(Room), roomId);
            var building = await dbContext.Buildings
                .AsNoTracking()
                .SingleOrDefaultAsync(candidate => candidate.Id == room.BuildingId && !candidate.IsDeleted, cancellationToken)
                ?? throw new NotFoundException(nameof(Building), room.BuildingId);

            resident.CheckIn(room, checkInDate);
            roomNumber = room.Number;
            buildingName = building.Name;
        }

        dbContext.Users.Add(residentUser);
        dbContext.Residents.Add(resident);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("CreateResident", nameof(Resident), resident.Id.ToString(), null,
            new { residentUser.Email, residentUser.FullName, resident.PassportNumber, resident.BirthDate, resident.Organization },
            cancellationToken);

        return new ResidentListItemDto(resident.Id, residentUser.FullName, residentUser.Email, residentUser.Phone,
            resident.Status, buildingName, roomNumber, resident.Balance, residentUser.IsActive);
    }
}
