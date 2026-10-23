using MediatR;

namespace BehaAtel.Application.Features.Me.GetMyProfile;

public sealed record GetMyProfileQuery : IRequest<MyProfileDto>;
