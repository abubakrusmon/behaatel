using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Admins.UpdateAdminPermissions;

public sealed record UpdateAdminPermissionsCommand(Guid AdminId, Permission[] Permissions) : IRequest;
