using System.Text.Json;
using Fix.Storage.Application.Abstractions.Processing;
using Fix.Storage.Application.Common.Interfaces.UseCases;
using Fix.Storage.Application.Files.Commands;
using Fix.Storage.Application.Files.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Fix.Storage.Infrastructure.Messaging;

/// <summary>
/// Consumidor de uma fila: cada mensagem roda num escopo próprio e só é confirmada (ack) depois de processada.
/// Falha passageira volta para a fila após uma pausa; mensagem ilegível ou erro repetido vai para a dead-letter.
/// </summary>
internal abstract class RabbitMqConsumer<TMessage>(
    RabbitMqConnection rabbit,
    IServiceScopeFactory scopes,
    ILogger logger) : BackgroundService
{
    protected abstract string Queue { get; }

    protected abstract ushort Prefetch { get; }

    protected abstract Task HandleAsync(IFileProcessingService processing, TMessage message, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await rabbit.GetAsync(stoppingToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await channel.BasicQosAsync(0, Prefetch, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) => await OnMessageAsync(channel, delivery, stoppingToken);
        await channel.BasicConsumeAsync(Queue, autoAck: false, consumer, stoppingToken);
        logger.LogInformation("Consumindo a fila {Queue}", Queue);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            await channel.DisposeAsync();
        }
    }

    private async Task OnMessageAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken stoppingToken)
    {
        TMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<TMessage>(delivery.Body.Span, RabbitMqConnection.Json);
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Mensagem ilegível na fila {Queue}", Queue);
            message = default;
        }

        if (message is null)
        {
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            return;
        }

        try
        {
            await using var scope = scopes.CreateAsyncScope();
            await HandleAsync(scope.ServiceProvider.GetRequiredService<IFileProcessingService>(), message, stoppingToken);
            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Desligando: a mensagem volta para a fila e é tratada na próxima subida.
        }
        catch (TransientLineException)
        {
            // Nova tentativa da linha (o limite de tentativas fica na aplicação).
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: true, stoppingToken);
        }
        catch (Exception exception)
        {
            // Erro de infraestrutura (Mongo, S3): uma nova tentativa; na segunda falha vai para a dead-letter.
            logger.LogError(exception, "Falha ao tratar mensagem da fila {Queue} (reentregue: {Redelivered})", Queue, delivery.Redelivered);
            if (!delivery.Redelivered)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }

            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: !delivery.Redelivered, stoppingToken);
        }
    }
}

/// <summary>FileUpload: lê o arquivo, grava as linhas e publica uma mensagem por linha.</summary>
internal sealed class FileUploadConsumer(RabbitMqConnection rabbit, IServiceScopeFactory scopes, ILogger<FileUploadConsumer> logger)
    : RabbitMqConsumer<FileUpload>(rabbit, scopes, logger)
{
    protected override string Queue => RabbitMqConnection.FileUploadedQueue;

    protected override ushort Prefetch => 1;

    protected override Task HandleAsync(IFileProcessingService processing, FileUpload message, CancellationToken cancellationToken) =>
        processing.IngestFileAsync(new IngestFileCommand(message.FileId), cancellationToken);
}

/// <summary>FileLineReceived: executa a linha no core-service.</summary>
internal sealed class FileLineConsumer(RabbitMqConnection rabbit, IServiceScopeFactory scopes, ILogger<FileLineConsumer> logger)
    : RabbitMqConsumer<FileLineReceived>(rabbit, scopes, logger)
{
    private readonly ushort prefetch = Math.Max((ushort)1, rabbit.Options.LinePrefetch);

    protected override string Queue => RabbitMqConnection.FileLinesQueue;

    protected override ushort Prefetch => prefetch;

    protected override Task HandleAsync(IFileProcessingService processing, FileLineReceived message, CancellationToken cancellationToken) =>
        processing.ProcessFileLineAsync(new ProcessFileLineCommand(message.FileId, message.LineId), cancellationToken);
}
