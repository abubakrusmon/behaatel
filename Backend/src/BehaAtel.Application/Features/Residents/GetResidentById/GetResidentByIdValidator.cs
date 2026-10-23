using FluentValidation;

namespace BehaAtel.Application.Features.Residents.GetResidentById;

public sealed class GetResidentByIdValidator : AbstractValidator<GetResidentByIdQuery>
{
    public GetResidentByIdValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
