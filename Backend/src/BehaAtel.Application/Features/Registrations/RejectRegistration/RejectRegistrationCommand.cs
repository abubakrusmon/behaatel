using MediatR;

namespace BehaAtel.Application.Features.Registrations.RejectRegistration;

public sealed record RejectRegistrationCommand(Guid UserId, string? Reason) : IRequest;
