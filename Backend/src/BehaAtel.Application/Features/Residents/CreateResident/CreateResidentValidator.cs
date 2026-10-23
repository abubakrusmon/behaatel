using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Validation;
using FluentValidation;

namespace BehaAtel.Application.Features.Residents.CreateResident;

public sealed class CreateResidentValidator : AbstractValidator<CreateResidentCommand>
{
    public CreateResidentValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Phone).MaximumLength(50);
        RuleFor(command => command.Password).RequireStrongPassword();
        RuleFor(command => command.PassportNumber).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Organization).MaximumLength(300);
        RuleFor(command => command.BirthDate)
            .Must(birthDate => birthDate <= DateOnly.FromDateTime(dateTimeProvider.UtcNow).AddYears(-14))
            .WithMessage("Жильцу должно быть не меньше 14 лет.");
        RuleFor(command => command)
            .Must(command => command.RoomId.HasValue == command.CheckInDate.HasValue)
            .WithMessage("Для заселения необходимо одновременно указать комнату и дату заселения.");
    }
}
