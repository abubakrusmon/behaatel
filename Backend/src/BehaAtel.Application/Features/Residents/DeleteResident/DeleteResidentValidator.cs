using FluentValidation;

namespace BehaAtel.Application.Features.Residents.DeleteResident;

public sealed class DeleteResidentValidator : AbstractValidator<DeleteResidentCommand>
{
    public DeleteResidentValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
