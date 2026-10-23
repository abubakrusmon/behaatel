using BehaAtel.Domain.Entities;

namespace BehaAtel.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId, string? ipAddress);
}
