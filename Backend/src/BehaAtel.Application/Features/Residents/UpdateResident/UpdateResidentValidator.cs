using BehaAtel.Application.Common.Interfaces;
using FluentValidation;

namespace BehaAtel.Application.Features.Residents.UpdateResident;

public sealed class UpdateResidentValidator : AbstractValidator<UpdateResidentCommand>
{
    public UpdateResidentValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Phone).MaximumLength(50);
        RuleFor(command => command.PassportNumber).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Organization).MaximumLength(300);
        RuleFor(command => command.BirthDate)
            .Must(birthDate => birthDate <= DateOnly.FromDateTime(dateTimeProvider.UtcNow).AddYears(-14))
            .WithMessage("Жильцу должно быть не меньше 14 лет.");
    }
}
