using BehaAtel.Application.Features.Rooms.CreateRoom;
using BehaAtel.Domain.Enums;
using FluentAssertions;

namespace BehaAtel.Application.UnitTests.Features.Rooms;

public class CreateRoomValidatorTests
{
    private readonly CreateRoomValidator _validator = new();

    [Fact]
    public void Validate_ValidRoom_IsValid()
    {
        var result = _validator.Validate(new CreateRoomCommand(
            Guid.NewGuid(), "101", 1, 4, 120m, RoomType.Standard));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_CapacityOutsideAllowedRange_IsInvalid()
    {
        var result = _validator.Validate(new CreateRoomCommand(
            Guid.NewGuid(), "101", 1, 21, 120m, RoomType.Standard));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateRoomCommand.Capacity));
    }

    [Fact]
    public void Validate_NegativePrice_IsInvalid()
    {
        var result = _validator.Validate(new CreateRoomCommand(
            Guid.NewGuid(), "101", 1, 4, -1m, RoomType.Standard));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateRoomCommand.PricePerBed));
    }
}
