using FluentValidation;

namespace BehaAtel.Application.Features.Announcements.GetAnnouncements;

public sealed class GetAnnouncementsValidator : AbstractValidator<GetAnnouncementsQuery>
{
    public GetAnnouncementsValidator()
    {
        RuleFor(query => query.Pagination).NotNull();
    }
}
