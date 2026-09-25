using Fix.Application.Organizations.Commands;
using FluentValidation;

namespace Fix.Application.Organizations.Validators;

internal sealed class AddMemberValidator : AbstractValidator<AddMemberCommand>
{
    public AddMemberValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.RuleCode).NotEmpty();
        RuleFor(x => x.Desk).IsInEnum().When(x => x.Desk is not null);
    }
}
