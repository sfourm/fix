using Fix.Storage.Application.Files.Commands;

namespace Fix.Storage.Application.Common.Interfaces.UseCases;

/// <summary>
/// Processamento assíncrono dos arquivos, em duas etapas desacopladas pelo RabbitMQ (chamadas pelos consumidores):
/// 1) <see cref="IngestFileAsync"/> (FileUpload): lê o arquivo, grava cada linha no MongoDB e publica uma mensagem
///    por linha — só distribui, não executa nada;
/// 2) <see cref="ProcessFileLineAsync"/> (FileLineReceived): executa a linha no core-service, grava o resultado e
///    publica o progresso. Uma linha que falha não afeta as outras.
/// </summary>
public interface IFileProcessingService
{
    Task IngestFileAsync(IngestFileCommand command, CancellationToken cancellationToken);

    Task ProcessFileLineAsync(ProcessFileLineCommand command, CancellationToken cancellationToken);
}
