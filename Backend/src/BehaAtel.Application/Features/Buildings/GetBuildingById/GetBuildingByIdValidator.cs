using FluentValidation;

namespace BehaAtel.Application.Features.Buildings.GetBuildingById;

public sealed class GetBuildingByIdValidator : AbstractValidator<GetBuildingByIdQuery>
{
    public GetBuildingByIdValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
