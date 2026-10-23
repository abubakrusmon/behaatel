using BehaAtel.Application.Features.Auth.Common;
using MediatR;

namespace BehaAtel.Application.Features.Auth.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<UserInfoDto>;
