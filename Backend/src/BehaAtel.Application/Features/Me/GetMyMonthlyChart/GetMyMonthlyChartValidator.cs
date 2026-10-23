using FluentValidation;

namespace BehaAtel.Application.Features.Me.GetMyMonthlyChart;

public sealed class GetMyMonthlyChartValidator : AbstractValidator<GetMyMonthlyChartQuery>
{
    public GetMyMonthlyChartValidator()
    {
        RuleFor(query => query.Months).InclusiveBetween(1, 24);
    }
}
