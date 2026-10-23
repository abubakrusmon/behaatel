using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Registrations.Common;

public sealed record RegistrationRequestDto(
    Guid UserId,
    string FullName,
    string Email,
    string? Phone,
    DateTime? RequestedAt,
    ApprovalStatus Status,
    bool EmailConfirmed);
