using BehaAtel.Domain.Entities;

namespace BehaAtel.Application.Features.Residents.Common;

public sealed record ResidentBalance(Resident Resident, decimal Balance);
