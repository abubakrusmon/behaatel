using FluentValidation;

namespace BehaAtel.Application.Features.Announcements.UpdateAnnouncement;

public sealed class UpdateAnnouncementValidator : AbstractValidator<UpdateAnnouncementCommand>
{
    public UpdateAnnouncementValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Title).NotEmpty().MinimumLength(3).MaximumLength(150);
        RuleFor(command => command.Body).NotEmpty().MaximumLength(5000);
    }
}
