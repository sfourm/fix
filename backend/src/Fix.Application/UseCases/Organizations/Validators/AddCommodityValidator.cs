using Fix.Application.Organizations.Commands;
using FluentValidation;

namespace Fix.Application.Organizations.Validators;

internal sealed class AddCommodityValidator : AbstractValidator<AddCommodityCommand>
{
    public AddCommodityValidator()
    {
        RuleFor(x => x.Commodity).IsInEnum();
        RuleFor(x => x.Unit).IsInEnum();
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
