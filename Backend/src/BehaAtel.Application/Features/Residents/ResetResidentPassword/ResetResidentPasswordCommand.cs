using MediatR;

namespace BehaAtel.Application.Features.Residents.ResetResidentPassword;

public sealed record ResetResidentPasswordCommand(Guid ResidentId, string NewPassword) : IRequest;
