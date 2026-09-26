namespace Fix.Storage.Domain.AggregateRoots.Files;

/// <summary>
/// Arquivo enviado por um usuário de uma organização: onde está (S3), o tipo e os contadores do processamento. As linhas
/// ficam em <see cref="FileLine"/>. Status e contadores são alterados de forma atômica pelo repositório (várias linhas
/// podem terminar ao mesmo tempo, em consumidores diferentes).
/// </summary>
public sealed class File : AggregateRoot
{
    private File()
    {
    }

    public Guid OrganizationId { get; private set; }

    public Guid UploadedBy { get; private set; }

    public FileKind Kind { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }

    /// <summary>Chave do objeto no bucket.</summary>
    public string StorageKey { get; private set; } = string.Empty;

    /// <summary>s3://bucket/chave</summary>
    public string StorageUrl { get; private set; } = string.Empty;

    public FileStatus Status { get; private set; }

    public DateTimeOffset UploadedAt { get; private set; }

    public DateTimeOffset? FinishedAt { get; private set; }

    public int TotalLines { get; private set; }

    public int ProcessedLines { get; private set; }

    public int SucceededLines { get; private set; }

    public int FailedLines { get; private set; }

    /// <summary>Falha do arquivo inteiro (ex.: não pôde ser lido).</summary>
    public string? Error { get; private set; }

    public bool IsFinished => Status is FileStatus.Completed or FileStatus.CompletedWithErrors or FileStatus.Failed or FileStatus.Stored;

    public int ProgressPercent => TotalLines == 0 ? (IsFinished ? 100 : 0) : (int)Math.Floor(ProcessedLines * 100m / TotalLines);

    /// <summary>Recebido: os tipos processados vão para a fila de leitura; os demais já terminam armazenados.</summary>
    public static File Receive(
        Guid organizationId,
        Guid uploadedBy,
        FileKind kind,
        string fileName,
        string contentType,
        long sizeBytes,
        string storageKey,
        string storageUrl,
        DateTimeOffset now) => new()
    {
        OrganizationId = organizationId,
        UploadedBy = uploadedBy,
        Kind = kind,
        FileName = fileName,
        ContentType = contentType,
        SizeBytes = sizeBytes,
        StorageKey = storageKey,
        StorageUrl = storageUrl,
        UploadedAt = now,
        Status = kind.IsProcessed() ? FileStatus.Received : FileStatus.Stored,
        FinishedAt = kind.IsProcessed() ? null : now,
    };

    /// <summary>Linhas lidas: começa o processamento. Falso se o arquivo já saiu de Received (reentrega do evento).</summary>
    public bool StartProcessing(int totalLines)
    {
        if (Status != FileStatus.Received)
        {
            return false;
        }

        Status = FileStatus.Processing;
        TotalLines = totalLines;
        return true;
    }

    public void MarkFailed(string error, DateTimeOffset at)
    {
        Status = FileStatus.Failed;
        Error = error;
        FinishedAt = at;
    }

    /// <summary>Uma linha terminou; a última fecha o arquivo com o status final.</summary>
    public void RegisterLineResult(bool succeeded, DateTimeOffset at)
    {
        ProcessedLines++;
        if (succeeded) SucceededLines++;
        else FailedLines++;

        if (Status == FileStatus.Processing && ProcessedLines >= TotalLines)
        {
            Status = FinalStatus(FailedLines);
            FinishedAt = at;
        }
    }

    /// <summary>Status final a partir das linhas que falharam.</summary>
    public static FileStatus FinalStatus(int failedLines) => failedLines > 0 ? FileStatus.CompletedWithErrors : FileStatus.Completed;
}
