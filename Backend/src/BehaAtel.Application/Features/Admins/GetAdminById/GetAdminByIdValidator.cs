using FluentValidation;

namespace BehaAtel.Application.Features.Admins.GetAdminById;

public sealed class GetAdminByIdValidator : AbstractValidator<GetAdminByIdQuery>
{
    public GetAdminByIdValidator()
    {
        RuleFor(query => query.AdminId).NotEmpty();
    }
}
