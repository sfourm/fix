using Fix.Storage.Application.Files.Commands;
using FluentValidation;

namespace Fix.Storage.Application.Files.Validators;

internal sealed class UploadFileValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileValidator()
    {
        RuleFor(x => x.Kind).IsInEnum().WithMessage("Informe o tipo do arquivo.");
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255).WithMessage("Informe o nome do arquivo (até 255 caracteres).");
        RuleFor(x => x.Content).NotEmpty().WithMessage("O arquivo está vazio.");
        RuleFor(x => x.Content.Length)
            .LessThanOrEqualTo(UploadFileCommand.MaxBytes)
            .WithMessage($"O arquivo passa do limite de {UploadFileCommand.MaxBytes / 1024 / 1024} MB.");
    }
}
