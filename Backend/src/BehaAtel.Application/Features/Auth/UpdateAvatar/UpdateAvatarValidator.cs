using BehaAtel.Domain.Entities;
using FluentValidation;

namespace BehaAtel.Application.Features.Auth.UpdateAvatar;

public sealed class UpdateAvatarValidator : AbstractValidator<UpdateAvatarCommand>
{
    public UpdateAvatarValidator()
    {
        RuleFor(command => command.AvatarUrl)
            .MaximumLength(User.MaxAvatarLength)
            .WithMessage("Изображение слишком большое.");
    }
}
