using BehaAtel.Application.Features.Buildings.CreateBuilding;
using FluentAssertions;

namespace BehaAtel.Application.UnitTests.Features.Buildings;

public class CreateBuildingValidatorTests
{
    private readonly CreateBuildingValidator _validator = new();

    [Fact]
    public void Validate_ValidBuilding_IsValid()
    {
        var result = _validator.Validate(new CreateBuildingCommand("Корпус А", "Главная улица, 1", 5));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Адрес", 5)]
    [InlineData("Корпус", "", 5)]
    [InlineData("Корпус", "Адрес", 0)]
    [InlineData("Корпус", "Адрес", 101)]
    public void Validate_InvalidBuilding_IsInvalid(string name, string address, int floorsCount)
    {
        var result = _validator.Validate(new CreateBuildingCommand(name, address, floorsCount));

        result.IsValid.Should().BeFalse();
    }
}
