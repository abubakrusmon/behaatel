using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Auth.UpdateAvatar;

public sealed class UpdateAvatarHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<UpdateAvatarCommand, UserInfoDto>
{
    public async Task<UserInfoDto> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new UnauthorizedException("Пользователь не авторизован.");
        }

        var user = await dbContext.Users
            .Include(candidate => candidate.Permissions)
            .SingleOrDefaultAsync(candidate => candidate.Id == userId && !candidate.IsDeleted, cancellationToken)
            ?? throw new UnauthorizedException("Пользователь не авторизован.");

        try
        {
            user.SetAvatar(request.AvatarUrl);
        }
        catch (DomainException exception)
        {
            throw new ConflictException(exception.Message);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return UserInfoDto.FromUser(user);
    }
}
