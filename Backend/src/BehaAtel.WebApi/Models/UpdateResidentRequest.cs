namespace BehaAtel.WebApi.Models;

public sealed record UpdateResidentRequest(
    string FullName,
    string? Phone,
    string PassportNumber,
    DateOnly BirthDate,
    string? Organization);
