using BehaAtel.Application.Common.Models;
using FluentValidation;

namespace BehaAtel.Application.Features.Notifications.CreateNotification;

public sealed class CreateNotificationValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationValidator()
    {
        RuleFor(command => command.Title).NotEmpty().MinimumLength(3).MaximumLength(150);
        RuleFor(command => command.Body).NotEmpty().MaximumLength(5000);
        RuleFor(command => command.AudienceKind).IsInEnum();
        RuleFor(command => command.BuildingId)
            .NotEmpty()
            .When(command => command.AudienceKind == TelegramAudienceKind.Building);
        RuleFor(command => command.ResidentIds)
            .NotEmpty()
            .When(command => command.AudienceKind == TelegramAudienceKind.SelectedResidents);
    }
}
