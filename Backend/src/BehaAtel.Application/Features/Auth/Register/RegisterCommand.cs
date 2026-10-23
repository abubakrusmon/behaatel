using BehaAtel.Application.Features.Auth.Common;
using MediatR;

namespace BehaAtel.Application.Features.Auth.Register;

public sealed record RegisterCommand(
    string Email,
    string FullName,
    string? Phone,
    string Password,
    string PassportNumber,
    DateOnly BirthDate,
    string? Organization) : IRequest<AuthActionDto>;

public sealed record AuthActionDto(string Message);
