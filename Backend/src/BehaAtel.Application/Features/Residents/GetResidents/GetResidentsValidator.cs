using BehaAtel.Domain.Enums;
using FluentValidation;

namespace BehaAtel.Application.Features.Residents.GetResidents;

public sealed class GetResidentsValidator : AbstractValidator<GetResidentsQuery>
{
    public GetResidentsValidator()
    {
        RuleFor(query => query.Pagination).NotNull();
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
    }
}
