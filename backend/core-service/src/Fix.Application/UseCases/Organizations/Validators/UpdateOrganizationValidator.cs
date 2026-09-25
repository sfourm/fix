using Fix.Application.Organizations.Commands;
using Fix.Domain.Common;
using FluentValidation;

namespace Fix.Application.Organizations.Validators;

internal sealed class UpdateOrganizationValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Name.MaxLength);
    }
}
