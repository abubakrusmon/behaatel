using FluentValidation;

namespace BehaAtel.Application.Features.Me.GetMyPayments;

public sealed class GetMyPaymentsValidator : AbstractValidator<GetMyPaymentsQuery>
{
    public GetMyPaymentsValidator()
    {
        RuleFor(query => query.Pagination).NotNull();
        RuleFor(query => query.From)
            .Must(from => !from.HasValue || from.Value.Kind == DateTimeKind.Utc)
            .WithMessage("Начальная дата должна быть в UTC.");
        RuleFor(query => query.To)
            .Must(to => !to.HasValue || to.Value.Kind == DateTimeKind.Utc)
            .WithMessage("Конечная дата должна быть в UTC.");
        RuleFor(query => query)
            .Must(query => !query.From.HasValue || !query.To.HasValue || query.From <= query.To)
            .WithMessage("Начальная дата не может быть позже конечной.");
    }
}
