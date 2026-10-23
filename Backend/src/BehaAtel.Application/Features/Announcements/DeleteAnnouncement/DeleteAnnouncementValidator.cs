using FluentValidation;

namespace BehaAtel.Application.Features.Announcements.DeleteAnnouncement;

public sealed class DeleteAnnouncementValidator : AbstractValidator<DeleteAnnouncementCommand>
{
    public DeleteAnnouncementValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
