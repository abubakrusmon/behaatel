using FluentValidation;

namespace BehaAtel.Application.Features.Announcements.CreateAnnouncement;

public sealed class CreateAnnouncementValidator : AbstractValidator<CreateAnnouncementCommand>
{
    public CreateAnnouncementValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MinimumLength(3).MaximumLength(150);
        RuleFor(command => command.Body).NotEmpty().MaximumLength(5000);
    }
}
