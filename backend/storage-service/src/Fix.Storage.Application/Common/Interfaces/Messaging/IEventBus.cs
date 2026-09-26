using Fix.Storage.Application.Files.Events;

namespace Fix.Storage.Application.Abstractions.Messaging;

/// <summary>Publicação dos eventos de arquivos no RabbitMQ.</summary>
public interface IEventBus
{
    Task PublishAsync(FileUpload message, CancellationToken cancellationToken);

    Task PublishAsync(FileLineReceived message, CancellationToken cancellationToken);

    Task PublishAsync(FileProgress message, CancellationToken cancellationToken);
}
