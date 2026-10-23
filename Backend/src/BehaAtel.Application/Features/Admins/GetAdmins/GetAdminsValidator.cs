using FluentValidation;

namespace BehaAtel.Application.Features.Admins.GetAdmins;

public sealed class GetAdminsValidator : AbstractValidator<GetAdminsQuery>
{
    public GetAdminsValidator()
    {
        RuleFor(query => query.Pagination).NotNull();
    }
}
