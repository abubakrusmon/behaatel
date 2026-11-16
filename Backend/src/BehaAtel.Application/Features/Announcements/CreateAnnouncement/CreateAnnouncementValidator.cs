using FluentValidation;

namespace BehaAtel.Application.Features.Announcements.CreateAnnouncement;

public sealed class CreateAnnouncementValidator : AbstractValidator<CreateAnnouncementCommand>
{
    public CreateAnnouncementValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MinimumLength(3).MaximumLength(150);
        RuleFor(command => command.Body).NotEmpty().MaximumLength(5000);
        RuleFor(command => command.AudienceKind).IsInEnum();
        RuleFor(command => command.BuildingId).NotNull().NotEqual(Guid.Empty)
            .When(command => command.AudienceKind == BehaAtel.Domain.Enums.AnnouncementAudience.Building)
            .WithMessage("Выберите здание.");
        RuleFor(command => command.ResidentIds).NotEmpty()
            .When(command => command.AudienceKind == BehaAtel.Domain.Enums.AnnouncementAudience.SelectedResidents)
            .WithMessage("Выберите хотя бы одного жильца.");
    }
}
