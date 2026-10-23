using MediatR;

namespace BehaAtel.Application.Features.Admins.BlockAdmin;

public sealed record BlockAdminCommand(Guid AdminId) : IRequest;
