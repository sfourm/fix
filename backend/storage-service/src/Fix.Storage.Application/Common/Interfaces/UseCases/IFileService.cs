using Fix.Storage.Application.Files.Commands;
using Fix.Storage.Application.Files.Dtos;
using Fix.Storage.Application.Files.Queries;
using Fix.Storage.Domain.Abstractions;

namespace Fix.Storage.Application.Common.Interfaces.UseCases;

/// <summary>
/// Arquivos: envio (grava no S3, registra e publica o FileUpload) e consultas. A entrada é validada na presentation
/// (IValidationFactory) e autorizada pelo UseCaseGuard; a regra de cada tipo, pelo FilePermissions.
/// </summary>
public interface IFileService
{
    Task<FileDto> UploadFileAsync(UploadFileCommand command, CancellationToken cancellationToken);

    Task<FileDto> GetFileAsync(GetFileQuery query, CancellationToken cancellationToken);

    Task<PagedList<FileDto>> ListFilesAsync(ListFilesQuery query, CancellationToken cancellationToken);

    Task<PagedList<FileLineDto>> ListFileLinesAsync(ListFileLinesQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<FileKindSummaryDto>> GetFileSummaryAsync(GetFileSummaryQuery query, CancellationToken cancellationToken);

    Task<FileTemplateDto> GetFileTemplateAsync(GetFileTemplateQuery query, CancellationToken cancellationToken);

    Task<FileDownloadUrlDto> GetFileDownloadUrlAsync(GetFileDownloadUrlQuery query, CancellationToken cancellationToken);

    Task<FileContentDto> GetFileContentAsync(GetFileContentQuery query, CancellationToken cancellationToken);
}
