using FluentValidation;

namespace BehaAtel.Application.Features.Buildings.UpdateBuilding;

public sealed class UpdateBuildingValidator : AbstractValidator<UpdateBuildingCommand>
{
    public UpdateBuildingValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Address).NotEmpty().MaximumLength(500);
        RuleFor(command => command.FloorsCount).InclusiveBetween(1, 100);
    }
}
