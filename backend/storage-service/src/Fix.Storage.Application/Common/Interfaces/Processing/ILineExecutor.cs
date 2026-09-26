using Fix.Storage.Domain.AggregateRoots.Files;
using File = Fix.Storage.Domain.AggregateRoots.Files.File;

namespace Fix.Storage.Application.Abstractions.Processing;

/// <summary>Resultado da execução de uma linha no core.</summary>
public sealed record LineResult(bool Succeeded, string Message, string? ResultCode = null)
{
    public static LineResult Ok(string message, string? code = null) => new(true, message, code);

    public static LineResult Fail(string message) => new(false, message);
}

/// <summary>
/// Executa uma linha no core-service em nome de quem enviou o arquivo (as regras e permissões do core valem
/// normalmente). Erro de negócio vira <see cref="LineResult.Fail"/>; indisponibilidade lança
/// <see cref="TransientLineException"/>.
/// </summary>
public interface ILineExecutor
{
    Task<LineResult> ExecuteAsync(File file, FileLine line, CancellationToken cancellationToken);
}

/// <summary>Falha passageira (core fora do ar, timeout): a linha volta para a fila e é tentada de novo.</summary>
public sealed class TransientLineException(string message, Exception? inner = null) : Exception(message, inner);
