using FluentValidation;

namespace BehaAtel.Application.Features.Residents.BlockResident;

public sealed class BlockResidentValidator : AbstractValidator<BlockResidentCommand>
{
    public BlockResidentValidator()
    {
        RuleFor(command => command.ResidentId).NotEmpty();
    }
}
