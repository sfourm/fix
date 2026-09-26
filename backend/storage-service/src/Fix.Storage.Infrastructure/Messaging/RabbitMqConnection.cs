using System.Text.Json;
using System.Text.Json.Serialization;
using Fix.Storage.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Fix.Storage.Infrastructure.Messaging;

/// <summary>
/// Conexão única com o RabbitMQ e a topologia dos arquivos:
/// exchange topic "fix.files" → filas storage.file-uploaded (file.uploaded) e storage.file-lines (file.line.received),
/// ambas com dead-letter em "fix.files.dead" → storage.files.dead. O progresso (file.progress) é publicado na mesma
/// exchange; o BFF cria a própria fila para repassar ao navegador.
/// </summary>
internal sealed class RabbitMqConnection(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnection> logger) : IAsyncDisposable
{
    public const string FileUploaded = "file.uploaded";
    public const string FileLineReceived = "file.line.received";
    public const string FileProgress = "file.progress";

    public const string FileUploadedQueue = "storage.file-uploaded";
    public const string FileLinesQueue = "storage.file-lines";
    public const string DeadQueue = "storage.files.dead";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private IConnection? connection;

    public RabbitMqOptions Options { get; } = options.Value;

    public string DeadExchange => Options.Exchange + ".dead";

    /// <summary>Conecta (com novas tentativas enquanto o broker sobe) e declara a topologia.</summary>
    public async Task<IConnection> GetAsync(CancellationToken cancellationToken)
    {
        if (connection is { IsOpen: true })
        {
            return connection;
        }

        await gate.WaitAsync(cancellationToken);
        try
        {
            if (connection is { IsOpen: true })
            {
                return connection;
            }

            var factory = new ConnectionFactory
            {
                Uri = new Uri(Options.Uri),
                ClientProvidedName = "fix-storage-service",
                AutomaticRecoveryEnabled = true,
            };

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    connection = await factory.CreateConnectionAsync(cancellationToken);
                    break;
                }
                catch (Exception exception) when (exception is not OperationCanceledException && attempt < 30)
                {
                    logger.LogWarning("RabbitMQ indisponível ({Message}); nova tentativa em 2s", exception.Message);
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }
            }

            await DeclareTopologyAsync(connection, cancellationToken);
            return connection;
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task DeclareTopologyAsync(IConnection current, CancellationToken cancellationToken)
    {
        await using var channel = await current.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(Options.Exchange, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(DeadExchange, ExchangeType.Fanout, durable: true, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(DeadQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(DeadQueue, DeadExchange, string.Empty, cancellationToken: cancellationToken);

        var arguments = new Dictionary<string, object?> { ["x-dead-letter-exchange"] = DeadExchange };
        foreach (var (queue, routingKey) in new[] { (FileUploadedQueue, FileUploaded), (FileLinesQueue, FileLineReceived) })
        {
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, arguments, cancellationToken: cancellationToken);
            await channel.QueueBindAsync(queue, Options.Exchange, routingKey, cancellationToken: cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (connection is not null)
        {
            await connection.DisposeAsync();
        }

        gate.Dispose();
    }
}
