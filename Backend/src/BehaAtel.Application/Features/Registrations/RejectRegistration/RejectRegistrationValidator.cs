using FluentValidation;

namespace BehaAtel.Application.Features.Registrations.RejectRegistration;

public sealed class RejectRegistrationValidator : AbstractValidator<RejectRegistrationCommand>
{
    public RejectRegistrationValidator() => RuleFor(command => command.Reason).MaximumLength(1000);
}
