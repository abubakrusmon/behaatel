using BehaAtel.Domain.Enums;
using FluentValidation;

namespace BehaAtel.Application.Features.Payments.GetPayments;

public sealed class GetPaymentsValidator : AbstractValidator<GetPaymentsQuery>
{
    public GetPaymentsValidator()
    {
        RuleFor(query => query.Pagination).NotNull();
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
        RuleFor(query => query.Method).IsInEnum().When(query => query.Method.HasValue);
        RuleFor(query => query.From)
            .Must(from => !from.HasValue || from.Value.Kind == DateTimeKind.Utc)
            .WithMessage("Начальная дата должна быть в UTC.");
        RuleFor(query => query.To)
            .Must(to => !to.HasValue || to.Value.Kind == DateTimeKind.Utc)
            .WithMessage("Конечная дата должна быть в UTC.");
        RuleFor(query => query)
            .Must(query => !query.From.HasValue || !query.To.HasValue || query.From <= query.To)
            .WithMessage("Начальная дата не может быть позже конечной.");
        RuleFor(query => query.MinAmount)
            .GreaterThanOrEqualTo(0)
            .When(query => query.MinAmount.HasValue);
        RuleFor(query => query.MaxAmount)
            .GreaterThanOrEqualTo(0)
            .When(query => query.MaxAmount.HasValue);
        RuleFor(query => query)
            .Must(query => !query.MinAmount.HasValue || !query.MaxAmount.HasValue || query.MinAmount <= query.MaxAmount)
            .WithMessage("Минимальная сумма не может быть больше максимальной.");
    }
}
