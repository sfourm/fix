using Fix.Application.Policies.Commands;
using Fix.Domain.Common;
using FluentValidation;

namespace Fix.Application.Policies.Validators;

internal sealed class CreatePolicyValidator : AbstractValidator<CreatePolicyCommand>
{
    public CreatePolicyValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Title.MaxLength);
        RuleFor(x => x.Version).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(DomainGuard.DescriptionMaxLength);
        RuleFor(x => x.ValidTo).GreaterThanOrEqualTo(x => x.ValidFrom).When(x => x.ValidTo is not null);
    }
}
