using FluentValidation;

namespace BehaAtel.Application.Features.Dashboard.GetRecentPayments;

public sealed class GetRecentPaymentsValidator : AbstractValidator<GetRecentPaymentsQuery>
{
    public GetRecentPaymentsValidator()
    {
        RuleFor(query => query.Limit).InclusiveBetween(1, 50);
    }
}
