using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Residents.CreateResident;
using FluentAssertions;
using Moq;

namespace BehaAtel.Application.UnitTests.Features.Residents;

public class CreateResidentValidatorTests
{
    private readonly DateTime _utcNow = new(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc);
    private readonly CreateResidentValidator _validator;

    public CreateResidentValidatorTests()
    {
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.Setup(provider => provider.UtcNow).Returns(_utcNow);
        _validator = new CreateResidentValidator(dateTimeProvider.Object);
    }

    [Fact]
    public void Validate_ResidentAtLeastFourteenWithNoRoom_IsValid()
    {
        var result = _validator.Validate(new CreateResidentCommand(
            "resident@example.com",
            "Resident User",
            null,
            "StrongPass1!",
            "AB123456",
            new DateOnly(2012, 10, 19),
            null,
            null,
            null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ResidentYoungerThanFourteen_IsInvalid()
    {
        var result = _validator.Validate(new CreateResidentCommand(
            "resident@example.com",
            "Resident User",
            null,
            "StrongPass1!",
            "AB123456",
            new DateOnly(2012, 10, 20),
            null,
            null,
            null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateResidentCommand.BirthDate));
    }

    [Fact]
    public void Validate_RoomWithoutCheckInDate_IsInvalid()
    {
        var result = _validator.Validate(new CreateResidentCommand(
            "resident@example.com",
            "Resident User",
            null,
            "StrongPass1!",
            "AB123456",
            new DateOnly(2010, 1, 1),
            null,
            Guid.NewGuid(),
            null));

        result.IsValid.Should().BeFalse();
    }
}
