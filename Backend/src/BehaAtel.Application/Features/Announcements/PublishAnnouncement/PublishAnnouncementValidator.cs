using FluentValidation;

namespace BehaAtel.Application.Features.Announcements.PublishAnnouncement;

public sealed class PublishAnnouncementValidator : AbstractValidator<PublishAnnouncementCommand>
{
    public PublishAnnouncementValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
