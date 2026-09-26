using Fix.Storage.Domain.AggregateRoots.Files;
using File = Fix.Storage.Domain.AggregateRoots.Files.File;

namespace Fix.Storage.Application.Files.Events;

/// <summary>Arquivo recebido e gravado (routing key file.uploaded).</summary>
public sealed record FileUpload(Guid FileId, Guid OrganizationId, Guid UserId, FileKind Kind);

/// <summary>Linha lida e gravada, pronta para executar (routing key file.line.received).</summary>
public sealed record FileLineReceived(Guid FileId, Guid LineId, int Number);

/// <summary>Resumo da linha que acabou de terminar (vai junto do progresso).</summary>
public sealed record FileProgressLine(int Number, FileLineStatus Status, string? Message, string? ResultCode);

/// <summary>Progresso do processamento (routing key file.progress; o BFF repassa ao navegador por SSE).</summary>
public sealed record FileProgress(
    Guid FileId,
    Guid OrganizationId,
    Guid UserId,
    FileKind Kind,
    string FileName,
    FileStatus Status,
    int TotalLines,
    int ProcessedLines,
    int SucceededLines,
    int FailedLines,
    int Percent,
    string? Error,
    FileProgressLine? Line)
{
    public static FileProgress From(File file, FileProgressLine? line = null) => new(
        file.Id,
        file.OrganizationId,
        file.UploadedBy,
        file.Kind,
        file.FileName,
        file.Status,
        file.TotalLines,
        file.ProcessedLines,
        file.SucceededLines,
        file.FailedLines,
        file.ProgressPercent,
        file.Error,
        line);
}
