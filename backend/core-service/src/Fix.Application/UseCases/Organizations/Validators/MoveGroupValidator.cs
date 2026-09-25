using Fix.Application.Organizations.Commands;
using FluentValidation;

namespace Fix.Application.Organizations.Validators;

internal sealed class MoveGroupValidator : AbstractValidator<MoveGroupCommand>
{
    public MoveGroupValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.ParentGroupId).NotEmpty().NotEqual(x => x.GroupId).WithMessage("Um grupo não pode ficar abaixo de si mesmo.");
    }
}
