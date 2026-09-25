using Fix.Application.Organizations.Commands;
using FluentValidation;

namespace Fix.Application.Organizations.Validators;

internal sealed class UpdateCommodityValidator : AbstractValidator<UpdateCommodityCommand>
{
    public UpdateCommodityValidator()
    {
        RuleFor(x => x.CommodityId).NotEmpty();
        RuleFor(x => x.Unit).IsInEnum();
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
