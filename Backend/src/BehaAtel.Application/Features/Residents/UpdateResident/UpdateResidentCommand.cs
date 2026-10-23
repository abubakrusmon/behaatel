using MediatR;

namespace BehaAtel.Application.Features.Residents.UpdateResident;

public sealed record UpdateResidentCommand(
    Guid Id,
    string FullName,
    string? Phone,
    string PassportNumber,
    DateOnly BirthDate,
    string? Organization) : IRequest;
