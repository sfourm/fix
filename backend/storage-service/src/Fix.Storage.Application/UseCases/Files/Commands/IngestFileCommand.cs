using Fix.Storage.Application.Abstractions.Messaging;

namespace Fix.Storage.Application.Files.Commands;

/// <summary>Consumo do FileUpload: lê o arquivo, grava as linhas e publica uma mensagem por linha.</summary>
public sealed record IngestFileCommand(Guid FileId) : ICommand;
