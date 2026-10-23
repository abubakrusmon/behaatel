using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Me.GetMyProfile;

public sealed class GetMyProfileHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetMyProfileQuery, MyProfileDto>
{
    public async Task<MyProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new ForbiddenException("Для текущего пользователя не найдена запись жильца.");
        }

        var profile = await (
            from resident in dbContext.Residents.AsNoTracking()
                .Where(resident => !resident.IsDeleted && resident.UserId == userId)
            join user in dbContext.Users.AsNoTracking()
                    .Where(user => !user.IsDeleted && user.Role == Role.User)
                on resident.UserId equals user.Id
            select new
            {
                user.FullName,
                user.Email,
                user.Phone,
                resident.PassportNumber,
                resident.Organization,
                resident.CheckInDate,
                resident.Status
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new ForbiddenException("Для текущего пользователя не найдена запись жильца.");

        return new MyProfileDto(
            profile.FullName,
            profile.Email,
            profile.Phone,
            MaskPassportNumber(profile.PassportNumber),
            profile.Organization,
            profile.CheckInDate,
            profile.Status);
    }

    private static string MaskPassportNumber(string passportNumber)
    {
        var visibleCharacters = Math.Min(3, passportNumber.Length);
        return new string('*', passportNumber.Length - visibleCharacters)
               + passportNumber[^visibleCharacters..];
    }
}
