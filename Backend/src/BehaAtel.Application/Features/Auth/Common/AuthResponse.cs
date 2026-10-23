namespace BehaAtel.Application.Features.Auth.Common;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfoDto User);
