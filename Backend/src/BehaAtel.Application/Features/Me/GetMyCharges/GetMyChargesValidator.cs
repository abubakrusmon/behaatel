using BehaAtel.Domain.Enums;
using FluentValidation;

namespace BehaAtel.Application.Features.Me.GetMyCharges;

public sealed class GetMyChargesValidator : AbstractValidator<GetMyChargesQuery>
{
    public GetMyChargesValidator()
    {
        RuleFor(query => query.Pagination).NotNull();
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
    }
}
