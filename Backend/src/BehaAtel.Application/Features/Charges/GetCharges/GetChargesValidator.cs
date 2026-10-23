using BehaAtel.Domain.Enums;
using FluentValidation;

namespace BehaAtel.Application.Features.Charges.GetCharges;

public sealed class GetChargesValidator : AbstractValidator<GetChargesQuery>
{
    public GetChargesValidator()
    {
        RuleFor(query => query.Pagination).NotNull();
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
        RuleFor(query => query.Type).IsInEnum().When(query => query.Type.HasValue);
    }
}
