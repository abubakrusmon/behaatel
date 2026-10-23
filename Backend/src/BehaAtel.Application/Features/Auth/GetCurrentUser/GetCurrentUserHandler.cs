using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetCurrentUserQuery, UserInfoDto>
{
    public async Task<UserInfoDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new UnauthorizedException("Пользователь не авторизован.");
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .Include(candidate => candidate.Permissions)
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);

        if (user is null || user.IsDeleted)
        {
            throw new UnauthorizedException("Пользователь не авторизован.");
        }

        return UserInfoDto.FromUser(user);
    }
}
