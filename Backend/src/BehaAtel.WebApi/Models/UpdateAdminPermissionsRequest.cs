using BehaAtel.Domain.Enums;

namespace BehaAtel.WebApi.Models;

public sealed record UpdateAdminPermissionsRequest(Permission[] Permissions);
