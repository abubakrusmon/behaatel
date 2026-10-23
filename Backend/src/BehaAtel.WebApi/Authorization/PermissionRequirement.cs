using BehaAtel.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace BehaAtel.WebApi.Authorization;

public sealed record PermissionRequirement(Permission Permission) : IAuthorizationRequirement;
