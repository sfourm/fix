using System.Text;
using Fix.Storage.Application.Files.Dtos;
using Fix.Storage.Domain.AggregateRoots.Files;
using Fix.Storage.Domain.AggregateRoots.Files.Repositories;
using Fix.Storage.Domain.AggregateRoots.Files.Templates;
using File = Fix.Storage.Domain.AggregateRoots.Files.File;

namespace Fix.Storage.Application.Files.Mappers;

internal static class FileMapper
{
    public static FileDto ToDto(this File file) => new(
        file.Id,
        file.Kind,
        file.Status,
        file.FileName,
        file.ContentType,
        file.SizeBytes,
        file.OrganizationId,
        file.UploadedBy,
        file.UploadedAt,
        file.FinishedAt,
        file.TotalLines,
        file.ProcessedLines,
        file.SucceededLines,
        file.FailedLines,
        file.Error,
        file.StorageUrl);

    public static FileLineDto ToDto(this FileLine line) => new(
        line.Id,
        line.Number,
        line.Status,
        line.Values,
        line.Message,
        line.ResultCode,
        line.ProcessedAt);

    public static FileKindSummaryDto ToDto(this FileKindSummary summary, bool canUpload) => new(
        summary.Kind,
        summary.Total,
        summary.Processing,
        summary.WithErrors,
        summary.LastUploadedAt,
        canUpload);

    public static FileTemplateDto ToDto(this FileTemplate template) => new(
        template.Kind,
        template.Processed,
        template.Extensions,
        [.. template.Columns.Select(c => new FileTemplateColumnDto(c.Name, c.Required, c.Description, c.Example))],
        // BOM: o Excel abre o CSV em UTF-8 com os acentos certos.
        template.Processed ? [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(template.ExampleCsv())] : []);
}
