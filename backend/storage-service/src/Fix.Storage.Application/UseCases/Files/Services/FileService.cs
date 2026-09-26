using Fix.Storage.Application.Abstractions.Exceptions;
using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Abstractions.Spreadsheets;
using Fix.Storage.Application.Abstractions.Storage;
using Fix.Storage.Application.Authorization;
using Fix.Storage.Application.Common;
using Fix.Storage.Application.Common.Interfaces.UseCases;
using Fix.Storage.Application.Files.Commands;
using Fix.Storage.Application.Files.Dtos;
using Fix.Storage.Application.Files.Events;
using Fix.Storage.Application.Files.Mappers;
using Fix.Storage.Application.Files.Queries;
using Fix.Storage.Domain.AggregateRoots.Files;
using Fix.Storage.Domain.AggregateRoots.Files.Repositories;
using Fix.Storage.Domain.AggregateRoots.Files.Templates;
using File = Fix.Storage.Domain.AggregateRoots.Files.File;

namespace Fix.Storage.Application.Files.Services;

internal sealed class FileService(
    FilePermissions permissions,
    IFileRepository fileRepository,
    IFileLineRepository fileLineRepository,
    IObjectStorage objectStorage,
    ITableReader tableReader,
    IEventBus eventBus,
    TimeProvider timeProvider) : IFileService
{
    private static readonly TimeSpan DownloadValidity = TimeSpan.FromMinutes(15);

    // ---------- Commands ----------

    /// <summary>
    /// Confere a regra do tipo, valida o arquivo contra o modelo, grava no S3, registra o arquivo (organização, usuário,
    /// tipo e URL) e publica o FileUpload. O processamento segue assíncrono.
    /// </summary>
    public async Task<FileDto> UploadFileAsync(UploadFileCommand command, CancellationToken cancellationToken)
    {
        var roles = await permissions.GetRolesAsync(command.OrganizationId, command.UserId, cancellationToken);
        FilePermissions.EnsureUpload(roles, command.Kind);

        var fileName = System.IO.Path.GetFileName(command.FileName.Trim());
        var extension = FileTemplates.EnsureExtension(command.Kind, fileName);
        if (command.Kind.IsProcessed())
        {
            // Confere o layout antes de gravar: arquivo fora do modelo nem entra na fila.
            EnsureRows(command.Kind, tableReader.Read(command.Content, extension));
        }

        var now = timeProvider.GetUtcNow();
        var key = $"{command.OrganizationId}/{command.Kind.ToString().ToLowerInvariant()}/{now:yyyy/MM}/{Guid.CreateVersion7()}{extension}";
        var contentType = string.IsNullOrWhiteSpace(command.ContentType) ? "application/octet-stream" : command.ContentType;
        var url = await objectStorage.PutAsync(key, command.Content, contentType, cancellationToken);

        var file = File.Receive(command.OrganizationId, command.UserId, command.Kind, fileName, contentType, command.Content.Length, key, url, now);
        await fileRepository.AddAsync(file, cancellationToken);
        await eventBus.PublishAsync(new FileUpload(file.Id, file.OrganizationId, file.UploadedBy, file.Kind), cancellationToken);

        return file.ToDto();
    }

    // ---------- Queries ----------

    public async Task<FileDto> GetFileAsync(GetFileQuery query, CancellationToken cancellationToken) =>
        (await GetVisibleAsync(query.OrganizationId, query.UserId, query.Id, cancellationToken)).ToDto();

    public async Task<PagedList<FileDto>> ListFilesAsync(ListFilesQuery query, CancellationToken cancellationToken)
    {
        var roles = await permissions.GetRolesAsync(query.OrganizationId, query.UserId, cancellationToken);
        if (query.Kind is { } kind)
        {
            FilePermissions.EnsureView(roles, kind);
        }

        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        var files = await fileRepository.ListAsync(query.OrganizationId, query.Kind, page, pageSize, cancellationToken);

        // Sem tipo: só o que o usuário pode ver.
        var visible = query.Kind is null
            ? files with { Items = [.. files.Items.Where(f => FilePermissions.CanView(roles, f.Kind))] }
            : files;
        return visible.Map(f => f.ToDto());
    }

    public async Task<PagedList<FileLineDto>> ListFileLinesAsync(ListFileLinesQuery query, CancellationToken cancellationToken)
    {
        var file = await GetVisibleAsync(query.OrganizationId, query.UserId, query.FileId, cancellationToken);
        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        var lines = await fileLineRepository.ListAsync(file.Id, query.Status, page, pageSize, cancellationToken);
        return lines.Map(l => l.ToDto());
    }

    public async Task<IReadOnlyList<FileKindSummaryDto>> GetFileSummaryAsync(GetFileSummaryQuery query, CancellationToken cancellationToken)
    {
        var roles = await permissions.GetRolesAsync(query.OrganizationId, query.UserId, cancellationToken);
        var summaries = (await fileRepository.SummaryAsync(query.OrganizationId, cancellationToken)).ToDictionary(s => s.Kind);

        return
        [
            .. Enum.GetValues<FileKind>()
                .Where(kind => FilePermissions.CanView(roles, kind))
                .Select(kind => (summaries.GetValueOrDefault(kind) ?? new FileKindSummary(kind, 0, 0, 0, null))
                    .ToDto(FilePermissions.CanUpload(roles, kind))),
        ];
    }

    public Task<FileTemplateDto> GetFileTemplateAsync(GetFileTemplateQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(FileTemplates.For(query.Kind).ToDto());

    public async Task<FileDownloadUrlDto> GetFileDownloadUrlAsync(GetFileDownloadUrlQuery query, CancellationToken cancellationToken)
    {
        var file = await GetVisibleAsync(query.OrganizationId, query.UserId, query.Id, cancellationToken);
        var (url, expiresAt) = objectStorage.GetDownloadUrl(file.StorageKey, file.FileName, DownloadValidity);
        return new FileDownloadUrlDto(url, expiresAt);
    }

    public async Task<FileContentDto> GetFileContentAsync(GetFileContentQuery query, CancellationToken cancellationToken)
    {
        var file = await GetVisibleAsync(query.OrganizationId, query.UserId, query.Id, cancellationToken);
        return new FileContentDto(file.FileName, file.ContentType, await objectStorage.GetAsync(file.StorageKey, cancellationToken));
    }

    // ---------- Helpers ----------

    /// <summary>Arquivo da organização que o usuário pode ver (de outra organização responde como inexistente).</summary>
    private async Task<File> GetVisibleAsync(Guid organizationId, Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var roles = await permissions.GetRolesAsync(organizationId, userId, cancellationToken);
        var file = await fileRepository.GetByIdAsync(id, cancellationToken);
        if (file is null || file.OrganizationId != organizationId)
        {
            throw new NotFoundException("Arquivo", id);
        }

        FilePermissions.EnsureView(roles, file.Kind);
        return file;
    }

    private static void EnsureRows(FileKind kind, Table table)
    {
        FileTemplates.EnsureHeader(kind, table.Header);
        var rows = table.Rows.Count(r => r.Any(v => !string.IsNullOrWhiteSpace(v)));
        if (rows == 0)
        {
            throw new BadRequestException("O arquivo não tem linhas de dados (só o cabeçalho).");
        }

        if (rows > UploadFileCommand.MaxRows)
        {
            throw new BadRequestException(
                $"O arquivo tem {rows:N0} linhas; o limite é {UploadFileCommand.MaxRows:N0}. Divida em arquivos menores.");
        }
    }
}
