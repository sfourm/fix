using Fix.Storage.Application.Files.Queries;
using FluentValidation;

namespace Fix.Storage.Application.Files.Validators;

internal sealed class GetFileValidator : AbstractValidator<GetFileQuery>
{
    public GetFileValidator() => RuleFor(x => x.Id).NotEmpty();
}

internal sealed class GetFileDownloadUrlValidator : AbstractValidator<GetFileDownloadUrlQuery>
{
    public GetFileDownloadUrlValidator() => RuleFor(x => x.Id).NotEmpty();
}

internal sealed class ListFilesValidator : AbstractValidator<ListFilesQuery>
{
    public ListFilesValidator() => RuleFor(x => x.Kind).IsInEnum().When(x => x.Kind is not null);
}

internal sealed class ListFileLinesValidator : AbstractValidator<ListFileLinesQuery>
{
    public ListFileLinesValidator()
    {
        RuleFor(x => x.FileId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status is not null);
    }
}

internal sealed class GetFileTemplateValidator : AbstractValidator<GetFileTemplateQuery>
{
    public GetFileTemplateValidator() => RuleFor(x => x.Kind).IsInEnum().WithMessage("Informe o tipo do arquivo.");
}
