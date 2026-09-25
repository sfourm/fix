using Fix.Application.Policies.Commands;
using Fix.Domain.Common;
using FluentValidation;

namespace Fix.Application.Policies.Validators;

internal sealed class UpdatePolicyValidator : AbstractValidator<UpdatePolicyCommand>
{
    public UpdatePolicyValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Title.MaxLength);
        RuleFor(x => x.Description).MaximumLength(DomainGuard.DescriptionMaxLength);
        RuleFor(x => x.ValidTo).GreaterThanOrEqualTo(x => x.ValidFrom).When(x => x.ValidTo is not null);
    }
}
