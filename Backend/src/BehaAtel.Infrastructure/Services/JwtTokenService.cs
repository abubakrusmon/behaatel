using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Security;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using BehaAtel.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BehaAtel.Infrastructure.Services;

public sealed class JwtTokenService(
    IOptions<JwtOptions> options,
    IDateTimeProvider dateTimeProvider) : IJwtTokenService
{
    public string GenerateAccessToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var settings = options.Value;
        ValidateAccessTokenOptions(settings);

        var now = dateTimeProvider.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var permissions = user.Role == Role.SuperAdmin
            ? Enum.GetValues<Permission>()
            : user.Permissions.Select(permission => permission.Permission);
        claims.AddRange(permissions.Select(permission =>
            new Claim("permission", PermissionNames.ToClaimValue(permission))));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            claims,
            now,
            now.AddMinutes(settings.AccessTokenExpirationMinutes),
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid userId, string? ipAddress)
    {
        _ = ipAddress;
        var settings = options.Value;
        if (settings.RefreshTokenExpirationDays <= 0)
        {
            throw new InvalidOperationException("Срок действия refresh token должен быть больше нуля.");
        }

        var now = dateTimeProvider.UtcNow;
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return RefreshToken.Create(
            userId,
            token,
            now.AddDays(settings.RefreshTokenExpirationDays),
            now);
    }

    private static void ValidateAccessTokenOptions(JwtOptions settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Issuer) ||
            string.IsNullOrWhiteSpace(settings.Audience) ||
            string.IsNullOrWhiteSpace(settings.Key) ||
            Encoding.UTF8.GetByteCount(settings.Key) < 32 ||
            settings.AccessTokenExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("Настройки JWT отсутствуют или некорректны.");
        }
    }
}
