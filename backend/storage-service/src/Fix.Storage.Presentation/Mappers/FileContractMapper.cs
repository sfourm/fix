using Fix.Storage.Application.Files.Dtos;
using Google.Protobuf;
using Contract = Fix.Contracts.V1;

namespace Fix.Storage.Presentation.Mappers;

internal static class FileContractMapper
{
    public static Contract.File ToContract(this FileDto file)
    {
        var contract = new Contract.File
        {
            Id = file.Id.ToString(),
            Kind = file.Kind.ToContract<Contract.FileKind>(),
            Status = file.Status.ToContract<Contract.FileStatus>(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            SizeBytes = file.SizeBytes,
            OrganizationId = file.OrganizationId.ToString(),
            UploadedBy = file.UploadedBy.ToString(),
            UploadedAt = file.UploadedAt.ToContract(),
            TotalLines = file.TotalLines,
            ProcessedLines = file.ProcessedLines,
            SucceededLines = file.SucceededLines,
            FailedLines = file.FailedLines,
            StorageUrl = file.StorageUrl,
        };

        if (file.FinishedAt is { } finished) contract.FinishedAt = finished.ToContract();
        if (file.Error is { } error) contract.Error = error;
        return contract;
    }

    public static Contract.FileLine ToContract(this FileLineDto line)
    {
        var contract = new Contract.FileLine
        {
            Id = line.Id.ToString(),
            Number = line.Number,
            Status = line.Status.ToContract<Contract.FileLineStatus>(),
            Values = { line.Values.ToDictionary() },
        };

        if (line.Message is { } message) contract.Message = message;
        if (line.ResultCode is { } code) contract.ResultCode = code;
        if (line.ProcessedAt is { } processed) contract.ProcessedAt = processed.ToContract();
        return contract;
    }

    public static Contract.FileKindSummary ToContract(this FileKindSummaryDto summary)
    {
        var contract = new Contract.FileKindSummary
        {
            Kind = summary.Kind.ToContract<Contract.FileKind>(),
            Total = summary.Total,
            Processing = summary.Processing,
            WithErrors = summary.WithErrors,
            CanUpload = summary.CanUpload,
        };

        if (summary.LastUploadedAt is { } last) contract.LastUploadedAt = last.ToContract();
        return contract;
    }

    public static Contract.FileTemplate ToContract(this FileTemplateDto template) => new()
    {
        Kind = template.Kind.ToContract<Contract.FileKind>(),
        Processed = template.Processed,
        AcceptedExtensions = { template.AcceptedExtensions },
        Columns =
        {
            template.Columns.Select(c => new Contract.FileTemplateColumn
            {
                Name = c.Name,
                Required = c.Required,
                Description = c.Description,
                Example = c.Example,
            }),
        },
        ExampleCsv = ByteString.CopyFrom(template.ExampleCsv),
    };

    public static Contract.FileDownloadUrl ToContract(this FileDownloadUrlDto download) => new()
    {
        Url = download.Url,
        ExpiresAt = download.ExpiresAt.ToContract(),
    };
}
