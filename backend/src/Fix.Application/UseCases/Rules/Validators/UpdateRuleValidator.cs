using Fix.Application.Rules.Commands;
using Fix.Domain.AggregateRoots.Roles;
using Fix.Domain.AggregateRoots.Rules;
using FluentValidation;

namespace Fix.Application.Rules.Validators;

internal sealed class UpdateRuleValidator : AbstractValidator<UpdateRuleCommand>
{
    public UpdateRuleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Rule.NameMaxLength);
        RuleFor(x => x.RoleCodes).NotEmpty().WithMessage("Escolha pelo menos uma role para a alçada.");
        RuleForEach(x => x.RoleCodes)
            .Must(SystemRoles.Descriptions.ContainsKey)
            .WithMessage("Role desconhecida: {PropertyValue}.");
    }
}
