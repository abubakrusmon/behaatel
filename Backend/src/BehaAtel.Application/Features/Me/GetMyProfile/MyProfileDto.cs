using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Me.GetMyProfile;

public sealed record MyProfileDto(
    string FullName,
    string Email,
    string? Phone,
    string PassportNumber,
    string? Organization,
    DateOnly? CheckInDate,
    ResidentStatus Status);
