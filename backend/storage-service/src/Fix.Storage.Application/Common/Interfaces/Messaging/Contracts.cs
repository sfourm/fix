namespace Fix.Storage.Application.Abstractions.Messaging;

/// <summary>
/// Entrada de um caso de uso da Application (command ou query). É por este marcador que a validação
/// (<see cref="Validation.IValidationFactory"/>) e a autorização tratam qualquer entrada de forma agnóstica.
/// </summary>
public interface IUseCase;

/// <summary>Operação que altera estado, sem retorno.</summary>
public interface ICommand : IUseCase;

/// <summary>Operação que altera estado e devolve um resultado.</summary>
public interface ICommand<out TResult> : ICommand;

/// <summary>Operação somente leitura.</summary>
public interface IQuery<out TResult> : IUseCase;
