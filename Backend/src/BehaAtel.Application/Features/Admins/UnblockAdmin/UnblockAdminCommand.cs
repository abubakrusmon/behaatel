using MediatR;

namespace BehaAtel.Application.Features.Admins.UnblockAdmin;

public sealed record UnblockAdminCommand(Guid AdminId) : IRequest;
