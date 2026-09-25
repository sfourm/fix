using Fix.Application.Organizations.Commands;
using Fix.Domain.Common;
using FluentValidation;

namespace Fix.Application.Organizations.Validators;

internal sealed class CreateGroupValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Name.MaxLength);
        RuleForEach(x => x.RuleCodes).NotEmpty();
    }
}
