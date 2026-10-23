using FluentValidation;

namespace BehaAtel.Application.Features.Dashboard.GetTopDebtors;

public sealed class GetTopDebtorsValidator : AbstractValidator<GetTopDebtorsQuery>
{
    public GetTopDebtorsValidator()
    {
        RuleFor(query => query.Limit).InclusiveBetween(1, 50);
    }
}
