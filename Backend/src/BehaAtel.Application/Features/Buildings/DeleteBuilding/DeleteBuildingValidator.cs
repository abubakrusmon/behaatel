using FluentValidation;

namespace BehaAtel.Application.Features.Buildings.DeleteBuilding;

public sealed class DeleteBuildingValidator : AbstractValidator<DeleteBuildingCommand>
{
    public DeleteBuildingValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
