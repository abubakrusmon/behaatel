using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Admins.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Admins.GetAdminById;

public sealed class GetAdminByIdHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetAdminByIdQuery, AdminDto>
{
    public async Task<AdminDto> Handle(GetAdminByIdQuery request, CancellationToken cancellationToken)
    {
        AdminAuthorization.RequireSuperAdmin(currentUser);
        var admin = await dbContext.Users
            .AsNoTracking()
            .Include(user => user.Permissions)
            .SingleOrDefaultAsync(user => user.Id == request.AdminId && user.Role == Role.Admin && !user.IsDeleted, cancellationToken);

        return admin is null
            ? throw new NotFoundException(nameof(User), request.AdminId)
            : AdminDto.FromUser(admin);
    }
}
