using System.Text.Json;
using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Files.Events;
using RabbitMQ.Client;

namespace Fix.Storage.Infrastructure.Messaging;

/// <summary>Publica os eventos em JSON (camelCase, enums como texto), persistentes e com confirmação do broker.</summary>
internal sealed class RabbitMqEventBus(RabbitMqConnection rabbit) : IEventBus, IAsyncDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private IChannel? channel;

    public Task PublishAsync(FileUpload message, CancellationToken cancellationToken) =>
        PublishAsync(RabbitMqConnection.FileUploaded, message, persistent: true, cancellationToken);

    public Task PublishAsync(FileLineReceived message, CancellationToken cancellationToken) =>
        PublishAsync(RabbitMqConnection.FileLineReceived, message, persistent: true, cancellationToken);

    // Progresso é descartável: se ninguém estiver ouvindo, não precisa sobreviver a um restart do broker.
    public Task PublishAsync(FileProgress message, CancellationToken cancellationToken) =>
        PublishAsync(RabbitMqConnection.FileProgress, message, persistent: false, cancellationToken);

    private async Task PublishAsync<T>(string routingKey, T message, bool persistent, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(message, RabbitMqConnection.Json);
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = persistent ? DeliveryModes.Persistent : DeliveryModes.Transient,
            Type = typeof(T).Name,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
        };

        // Um canal não aceita publicações simultâneas.
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (channel is not { IsOpen: true })
            {
                var connection = await rabbit.GetAsync(cancellationToken);
                channel = await connection.CreateChannelAsync(
                    new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
                    cancellationToken);
            }

            await channel.BasicPublishAsync(rabbit.Options.Exchange, routingKey, mandatory: false, properties, body, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (channel is not null)
        {
            await channel.DisposeAsync();
        }

        gate.Dispose();
    }
}
