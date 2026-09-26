namespace Fix.Storage.Application.Abstractions.Exceptions;

public abstract class AppException(string message) : Exception(message);

public sealed class NotFoundException(string resource, object key)
    : AppException($"{resource} '{key}' não encontrado(a).");

public sealed class ForbiddenException(string message = "Você não tem permissão para executar esta operação.")
    : AppException(message);

public sealed class BadRequestException(string message) : AppException(message);
