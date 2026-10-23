using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Enums;

namespace BehaAtel.WebApi.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                        ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public Role? Role
    {
        get
        {
            var value = Principal?.FindFirstValue(ClaimTypes.Role)
                        ?? Principal?.FindFirstValue("role");
            return Enum.TryParse<Role>(value, true, out var role) ? role : null;
        }
    }

    public string? Email =>
        Principal?.FindFirstValue(ClaimTypes.Email)
        ?? Principal?.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? Principal?.FindFirstValue("email");

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
