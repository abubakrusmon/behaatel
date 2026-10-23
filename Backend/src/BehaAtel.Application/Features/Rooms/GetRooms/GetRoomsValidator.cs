using BehaAtel.Domain.Enums;
using FluentValidation;

namespace BehaAtel.Application.Features.Rooms.GetRooms;

public sealed class GetRoomsValidator : AbstractValidator<GetRoomsQuery>
{
    public GetRoomsValidator()
    {
        RuleFor(query => query.Pagination).NotNull();
        RuleFor(query => query.Floor).GreaterThan(0).When(query => query.Floor.HasValue);
        RuleFor(query => query.Type).IsInEnum().When(query => query.Type.HasValue);
    }
}
