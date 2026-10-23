using FluentValidation;

namespace BehaAtel.Application.Features.Me.GetMyAnnouncements;

public sealed class GetMyAnnouncementsValidator : AbstractValidator<GetMyAnnouncementsQuery>
{
    public GetMyAnnouncementsValidator()
    {
        RuleFor(query => query.Pagination).NotNull();
    }
}
