namespace Fix.Storage.Domain.AggregateRoots.Files;

public enum FileStatus
{
    /// <summary>Gravado e na fila para leitura das linhas.</summary>
    Received = 1,

    Processing = 2,

    Completed = 3,

    CompletedWithErrors = 4,

    /// <summary>O arquivo inteiro não pôde ser lido.</summary>
    Failed = 5,

    /// <summary>Tipos só armazenados (políticas e documentos).</summary>
    Stored = 6,
}
