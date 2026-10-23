using FluentValidation;

namespace BehaAtel.Application.Features.Residents.CheckInResident;

public sealed class CheckInResidentValidator : AbstractValidator<CheckInResidentCommand>
{
    public CheckInResidentValidator()
    {
        RuleFor(command => command.ResidentId).NotEmpty();
        RuleFor(command => command.RoomId).NotEmpty();
    }
}
