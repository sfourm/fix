using Fix.Application.Organizations.Commands;
using FluentValidation;

namespace Fix.Application.Organizations.Validators;

internal sealed class UpdateCompanyProfileValidator : AbstractValidator<UpdateCompanyProfileCommand>
{
    public UpdateCompanyProfileValidator()
    {
        RuleFor(x => x.CorporateName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sector).IsInEnum();
        RuleFor(x => x.CropYearStartMonth).InclusiveBetween(1, 12);
        RuleFor(x => x.ActiveCrop).Matches(@"^\d{2}/\d{2}$").When(x => !string.IsNullOrWhiteSpace(x.ActiveCrop))
            .WithMessage("Safra deve estar no formato AA/AA (ex.: 26/27).");
    }
}
