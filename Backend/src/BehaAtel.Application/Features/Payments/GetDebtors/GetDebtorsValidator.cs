using FluentValidation;

namespace BehaAtel.Application.Features.Payments.GetDebtors;

public sealed class GetDebtorsValidator : AbstractValidator<GetDebtorsQuery>
{
    public GetDebtorsValidator()
    {
        RuleFor(query => query.Pagination).NotNull();
    }
}
