using MediatR;

namespace BehaAtel.Application.Features.Registrations.ApproveRegistration;

public sealed record ApproveRegistrationCommand(Guid UserId) : IRequest;
