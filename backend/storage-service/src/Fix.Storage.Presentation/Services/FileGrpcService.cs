using Fix.Contracts.V1;
using Fix.Storage.Application.Abstractions.Validation;
using Fix.Storage.Application.Common.Interfaces.UseCases;
using Fix.Storage.Application.Files.Commands;
using Fix.Storage.Application.Files.Queries;
using Fix.Storage.Presentation.Mappers;
using Fix.Storage.Presentation.UseCases;
using Grpc.Core;
using Entities = Fix.Storage.Domain.AggregateRoots.Files;
using File = Fix.Contracts.V1.File;

namespace Fix.Storage.Presentation.Services;

internal sealed class FileGrpcService(
    IValidationFactory validation,
    IFileService fileService) : FileService.FileServiceBase
{
    public override async Task<File> UploadFile(UploadFileRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new UploadFileCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Kind.ToDomain<Entities.FileKind>("kind"),
                request.FileName,
                request.ContentType,
                request.Content.ToByteArray()),
            fileService.UploadFileAsync,
            context.CancellationToken)).ToContract();

    public override async Task<File> GetFile(FileIdRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new GetFileQuery(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            fileService.GetFileAsync,
            context.CancellationToken)).ToContract();

    public override async Task<ListFilesResponse> ListFiles(ListFilesRequest request, ServerCallContext context)
    {
        var (page, pageSize) = request.Page.ToPaging();
        var files = await validation.RunAsync(
            new ListFilesQuery(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Kind.ToOptionalDomain<Entities.FileKind>("kind"),
                page,
                pageSize),
            fileService.ListFilesAsync,
            context.CancellationToken);

        return new ListFilesResponse { Files = { files.Items.Select(f => f.ToContract()) }, Page = files.ToPageInfo() };
    }

    public override async Task<ListFileLinesResponse> ListFileLines(ListFileLinesRequest request, ServerCallContext context)
    {
        var (page, pageSize) = request.Page.ToPaging();
        var lines = await validation.RunAsync(
            new ListFileLinesQuery(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.FileId.ToGuid("file_id"),
                request.Status.ToOptionalDomain<Entities.FileLineStatus>("status"),
                page,
                pageSize),
            fileService.ListFileLinesAsync,
            context.CancellationToken);

        return new ListFileLinesResponse { Lines = { lines.Items.Select(l => l.ToContract()) }, Page = lines.ToPageInfo() };
    }

    public override async Task<FileSummaryResponse> GetFileSummary(FileSummaryRequest request, ServerCallContext context)
    {
        var kinds = await validation.RunAsync(
            new GetFileSummaryQuery(request.Context.ToUserId(), request.Context.ToOrganizationId()),
            fileService.GetFileSummaryAsync,
            context.CancellationToken);

        return new FileSummaryResponse { Kinds = { kinds.Select(k => k.ToContract()) } };
    }

    public override async Task<FileTemplate> GetFileTemplate(GetFileTemplateRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new GetFileTemplateQuery(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Kind.ToDomain<Entities.FileKind>("kind")),
            fileService.GetFileTemplateAsync,
            context.CancellationToken)).ToContract();

    public override async Task<FileDownloadUrl> GetFileDownloadUrl(FileIdRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new GetFileDownloadUrlQuery(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            fileService.GetFileDownloadUrlAsync,
            context.CancellationToken)).ToContract();
}
