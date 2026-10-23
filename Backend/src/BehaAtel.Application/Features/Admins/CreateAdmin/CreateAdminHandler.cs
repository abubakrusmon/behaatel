using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Admins.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Admins.CreateAdmin;

public sealed class CreateAdminHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    IAuditService auditService)
    : IRequestHandler<CreateAdminCommand, AdminDto>
{
    public async Task<AdminDto> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        AdminAuthorization.RequireSuperAdmin(currentUser);
        var email = request.Email.Trim();
        var emailKey = email.ToLower();
        if (await dbContext.Users.AnyAsync(user => user.Email.ToLower() == emailKey, cancellationToken))
        {
            throw new ConflictException("Пользователь с таким email уже существует.");
        }

        var admin = User.Create(email, passwordHasher.Hash(request.Password), request.FullName, Role.Admin, request.Phone, mustChangePassword: true);
        admin.SetPermissions(request.Permissions);
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("CreateAdmin", nameof(User), admin.Id.ToString(), null,
            new { admin.Email, admin.FullName, admin.Phone, Permissions = request.Permissions }, cancellationToken);

        return AdminDto.FromUser(admin);
    }
}
