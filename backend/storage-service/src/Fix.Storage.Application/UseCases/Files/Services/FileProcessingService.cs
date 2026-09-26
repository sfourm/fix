using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Abstractions.Processing;
using Fix.Storage.Application.Abstractions.Spreadsheets;
using Fix.Storage.Application.Abstractions.Storage;
using Fix.Storage.Application.Common.Interfaces.UseCases;
using Fix.Storage.Application.Files.Commands;
using Fix.Storage.Application.Files.Events;
using Fix.Storage.Domain.AggregateRoots.Files;
using Fix.Storage.Domain.AggregateRoots.Files.Repositories;
using Fix.Storage.Domain.AggregateRoots.Files.Templates;
using Microsoft.Extensions.Logging;

namespace Fix.Storage.Application.Files.Services;

internal sealed class FileProcessingService(
    IFileRepository fileRepository,
    IFileLineRepository fileLineRepository,
    IObjectStorage objectStorage,
    ITableReader tableReader,
    ILineExecutor lineExecutor,
    IEventBus eventBus,
    TimeProvider timeProvider,
    ILogger<FileProcessingService> logger) : IFileProcessingService
{
    /// <summary>Tentativas de uma linha quando o core está indisponível, antes de marcá-la como falha.</summary>
    public const int MaxAttempts = 3;

    public async Task IngestFileAsync(IngestFileCommand command, CancellationToken cancellationToken)
    {
        var file = await fileRepository.GetByIdAsync(command.FileId, cancellationToken);
        // Tipos só armazenados não têm linhas; reentregas do mesmo evento não releem o arquivo.
        if (file is null || !file.Kind.IsProcessed() || file.Status != FileStatus.Received)
        {
            return;
        }

        List<FileLine> read;
        try
        {
            read = await ReadLinesAsync(file, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Arquivo {FileId} não pôde ser lido", file.Id);
            await fileRepository.FailAsync(file.Id, $"O arquivo não pôde ser lido: {exception.Message}", timeProvider.GetUtcNow(), cancellationToken);
            await PublishProgressAsync(file.Id, cancellationToken);
            return;
        }

        var lines = await fileLineRepository.AddManyAsync(read, cancellationToken);
        if (!await fileRepository.StartProcessingAsync(file.Id, lines.Count, cancellationToken))
        {
            return;
        }

        await PublishProgressAsync(file.Id, cancellationToken);
        foreach (var line in lines)
        {
            await eventBus.PublishAsync(new FileLineReceived(file.Id, line.Id, line.Number), cancellationToken);
        }
    }

    public async Task ProcessFileLineAsync(ProcessFileLineCommand command, CancellationToken cancellationToken)
    {
        var line = await fileLineRepository.GetByIdAsync(command.LineId, cancellationToken);
        // Linha já processada (mensagem reentregue): nada a fazer.
        if (line is not { IsPending: true })
        {
            return;
        }

        var file = await fileRepository.GetByIdAsync(command.FileId, cancellationToken);
        if (file is null)
        {
            return;
        }

        line.RegisterAttempt();
        LineResult result;
        try
        {
            result = await lineExecutor.ExecuteAsync(file, line, cancellationToken);
        }
        catch (TransientLineException exception) when (line.Attempts < MaxAttempts)
        {
            // Volta para a fila (o consumidor faz o nack); a tentativa fica registrada.
            await fileLineRepository.UpdateAsync(line, cancellationToken);
            logger.LogWarning(exception, "Linha {Line} do arquivo {FileId}: tentativa {Attempt} falhou", line.Number, file.Id, line.Attempts);
            throw;
        }
        catch (TransientLineException exception)
        {
            result = LineResult.Fail($"Serviço indisponível após {line.Attempts} tentativas: {exception.Message}");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Linha {Line} do arquivo {FileId}: erro inesperado", line.Number, file.Id);
            result = LineResult.Fail("Erro inesperado ao processar a linha.");
        }

        var now = timeProvider.GetUtcNow();
        if (result.Succeeded)
        {
            line.Succeed(result.Message, result.ResultCode, now);
        }
        else
        {
            line.Fail(result.Message, now);
        }

        await fileLineRepository.UpdateAsync(line, cancellationToken);

        var updated = await fileRepository.RegisterLineResultAsync(file.Id, result.Succeeded, now, cancellationToken);
        await eventBus.PublishAsync(
            FileProgress.From(updated, new FileProgressLine(line.Number, line.Status, line.Message, line.ResultCode)),
            cancellationToken);
    }

    /// <summary>Lê o arquivo do S3 e monta as linhas (coluna normalizada → valor), ignorando as vazias.</summary>
    private async Task<List<FileLine>> ReadLinesAsync(Domain.AggregateRoots.Files.File file, CancellationToken cancellationToken)
    {
        var content = await objectStorage.GetAsync(file.StorageKey, cancellationToken);
        var table = tableReader.Read(content, System.IO.Path.GetExtension(file.FileName).ToLowerInvariant());
        var header = FileTemplates.EnsureHeader(file.Kind, table.Header);

        var lines = new List<FileLine>();
        foreach (var row in table.Rows.Where(r => r.Any(v => !string.IsNullOrWhiteSpace(v))))
        {
            var values = new Dictionary<string, string>();
            for (var i = 0; i < header.Count; i++)
            {
                if (header[i].Length > 0)
                {
                    values[header[i]] = i < row.Count ? row[i].Trim() : string.Empty;
                }
            }

            lines.Add(FileLine.Create(file, lines.Count + 1, values));
        }

        return lines;
    }

    private async Task PublishProgressAsync(Guid fileId, CancellationToken cancellationToken)
    {
        if (await fileRepository.GetByIdAsync(fileId, cancellationToken) is { } file)
        {
            await eventBus.PublishAsync(FileProgress.From(file), cancellationToken);
        }
    }
}
