using BehaAtel.Domain.Enums;
using FluentValidation;

namespace BehaAtel.Application.Features.Rooms.CreateRoom;

public sealed class CreateRoomValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomValidator()
    {
        RuleFor(command => command.BuildingId).NotEmpty();
        RuleFor(command => command.Number).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Floor).GreaterThan(0);
        RuleFor(command => command.Capacity).InclusiveBetween(1, 20);
        RuleFor(command => command.PricePerBed).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Type).IsInEnum();
    }
}
