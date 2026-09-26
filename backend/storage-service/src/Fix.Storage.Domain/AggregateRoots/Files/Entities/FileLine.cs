namespace Fix.Storage.Domain.AggregateRoots.Files;

/// <summary>
/// Uma linha de dados do arquivo, como veio (coluna → valor), e o resultado da execução no core. Cada linha é
/// processada de forma independente: se uma falha, as demais seguem.
/// </summary>
public sealed class FileLine : Entity
{
    private FileLine()
    {
    }

    public Guid FileId { get; private set; }

    public Guid OrganizationId { get; private set; }

    /// <summary>1 = primeira linha de dados (o cabeçalho não conta).</summary>
    public int Number { get; private set; }

    public IReadOnlyDictionary<string, string> Values { get; private set; } = new Dictionary<string, string>();

    public FileLineStatus Status { get; private set; } = FileLineStatus.Pending;

    /// <summary>Erro da linha ou resumo do que foi feito.</summary>
    public string? Message { get; private set; }

    /// <summary>Código do que foi criado no core (ex.: MD-07, HX-0012).</summary>
    public string? ResultCode { get; private set; }

    public int Attempts { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public bool IsPending => Status == FileLineStatus.Pending;

    public static FileLine Create(File file, int number, IReadOnlyDictionary<string, string> values) => new()
    {
        FileId = file.Id,
        OrganizationId = file.OrganizationId,
        Number = number,
        Values = values,
    };

    /// <summary>Nova tentativa de execução (o limite fica no processamento).</summary>
    public void RegisterAttempt()
    {
        if (!IsPending)
        {
            throw new DomainException($"A linha {Number} já foi processada.");
        }

        Attempts++;
    }

    public void Succeed(string message, string? resultCode, DateTimeOffset at) =>
        Finish(FileLineStatus.Succeeded, message, resultCode, at);

    public void Fail(string message, DateTimeOffset at) =>
        Finish(FileLineStatus.Failed, message, null, at);

    private void Finish(FileLineStatus status, string message, string? resultCode, DateTimeOffset at)
    {
        if (!IsPending)
        {
            throw new DomainException($"A linha {Number} já foi processada.");
        }

        Status = status;
        Message = message;
        ResultCode = resultCode;
        ProcessedAt = at;
    }
}
