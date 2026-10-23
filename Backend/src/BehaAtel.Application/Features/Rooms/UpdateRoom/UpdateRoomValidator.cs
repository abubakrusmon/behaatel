using BehaAtel.Domain.Enums;
using FluentValidation;

namespace BehaAtel.Application.Features.Rooms.UpdateRoom;

public sealed class UpdateRoomValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Number).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Floor).GreaterThan(0);
        RuleFor(command => command.Capacity).InclusiveBetween(1, 20);
        RuleFor(command => command.PricePerBed).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Type).IsInEnum();
    }
}
