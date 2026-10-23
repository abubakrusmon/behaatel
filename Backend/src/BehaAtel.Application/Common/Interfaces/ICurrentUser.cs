using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Role? Role { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    string? IpAddress { get; }
}
