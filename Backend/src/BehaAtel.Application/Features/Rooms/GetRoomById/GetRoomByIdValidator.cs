using FluentValidation;

namespace BehaAtel.Application.Features.Rooms.GetRoomById;

public sealed class GetRoomByIdValidator : AbstractValidator<GetRoomByIdQuery>
{
    public GetRoomByIdValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
