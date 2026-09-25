using System.Text.Json;
using Fix.Application.Abstractions.Exceptions;
using Fix.Domain.Abstractions;
using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Fix.Presentation.Interceptors;

/// <summary>Traduz as exceções da aplicação/domínio para status gRPC.</summary>
internal sealed class ExceptionInterceptor(ILogger<ExceptionInterceptor> logger) : Interceptor
{
    private const string ValidationErrorsTrailer = "validation-errors";

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception exception) when (exception is not RpcException)
        {
            throw ToRpcException(exception, context);
        }
    }

    private RpcException ToRpcException(Exception exception, ServerCallContext context)
    {
        switch (exception)
        {
            case ValidationException validation:
            {
                var errors = validation.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                var trailers = new Metadata { { ValidationErrorsTrailer, JsonSerializer.Serialize(errors) } };
                var message = string.Join(" ", validation.Errors.Select(e => e.ErrorMessage));
                return new RpcException(new Status(StatusCode.InvalidArgument, message), trailers);
            }

            case BadRequestException:
                return Rpc(StatusCode.InvalidArgument, exception);
            case NotFoundException:
                return Rpc(StatusCode.NotFound, exception);
            case ConflictException:
                return Rpc(StatusCode.AlreadyExists, exception);
            case UnauthenticatedException:
                return Rpc(StatusCode.Unauthenticated, exception);
            case ForbiddenException:
                return Rpc(StatusCode.PermissionDenied, exception);
            case DomainException:
                return Rpc(StatusCode.FailedPrecondition, exception);
            case OperationCanceledException when context.CancellationToken.IsCancellationRequested:
                return Rpc(StatusCode.Cancelled, exception);
            default:
                logger.LogError(exception, "Erro não tratado em {Method}", context.Method);
                return new RpcException(new Status(StatusCode.Internal, "Erro interno do servidor."));
        }
    }

    private static RpcException Rpc(StatusCode code, Exception exception) =>
        new(new Status(code, exception.Message));
}
