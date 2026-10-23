using FluentValidation;

namespace BehaAtel.Application.Features.Dashboard.GetRevenueByMonth;

public sealed class GetRevenueByMonthValidator : AbstractValidator<GetRevenueByMonthQuery>
{
    public GetRevenueByMonthValidator()
    {
        RuleFor(query => query.Months).InclusiveBetween(1, 24);
    }
}
