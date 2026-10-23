using FluentValidation;

namespace BehaAtel.Application.Features.Rooms.DeleteRoom;

public sealed class DeleteRoomValidator : AbstractValidator<DeleteRoomCommand>
{
    public DeleteRoomValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
