namespace BehaAtel.Application.Features.Auth.Common;

public sealed record LoginResultDto(
    bool RequiresCode,
    AuthResponse? Session,
    Guid? ChallengeId,
    DateTime? ExpiresAt,
    string? MaskedEmail,
    TelegramLinkChallengeDto? TelegramLink = null);
