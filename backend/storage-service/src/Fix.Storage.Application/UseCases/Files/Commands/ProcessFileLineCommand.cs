using Fix.Storage.Application.Abstractions.Messaging;

namespace Fix.Storage.Application.Files.Commands;

/// <summary>Consumo do FileLineReceived: executa a linha no core-service.</summary>
public sealed record ProcessFileLineCommand(Guid FileId, Guid LineId) : ICommand;
