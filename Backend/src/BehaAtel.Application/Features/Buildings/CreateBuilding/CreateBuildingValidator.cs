using FluentValidation;

namespace BehaAtel.Application.Features.Buildings.CreateBuilding;

public sealed class CreateBuildingValidator : AbstractValidator<CreateBuildingCommand>
{
    public CreateBuildingValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Address).NotEmpty().MaximumLength(500);
        RuleFor(command => command.FloorsCount).InclusiveBetween(1, 100);
    }
}
