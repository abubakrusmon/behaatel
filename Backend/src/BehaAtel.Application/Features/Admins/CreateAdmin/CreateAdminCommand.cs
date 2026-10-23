using BehaAtel.Application.Features.Admins.Common;
using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Admins.CreateAdmin;

public sealed record CreateAdminCommand(
    string Email,
    string FullName,
    string? Phone,
    string Password,
    Permission[] Permissions) : IRequest<AdminDto>;
