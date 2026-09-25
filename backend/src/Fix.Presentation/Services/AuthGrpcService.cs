using Fix.Application.Abstractions.Messaging;
using Fix.Application.Auth.Commands;
using Fix.Application.Auth.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Grpc.Core;

namespace Fix.Presentation.Services;

internal sealed class AuthGrpcService(IDispatcher dispatcher) : AuthService.AuthServiceBase
{
    public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        var userId = await dispatcher.SendAsync(
            new RegisterUserCommand(request.Email, request.Password, request.FullName),
            context.CancellationToken);

        return new RegisterResponse { UserId = userId.ToString() };
    }

    public override async Task<User> Authenticate(AuthenticateRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new AuthenticateUserCommand(request.Email, request.Password),
            context.CancellationToken)).ToContract();

    public override async Task<User> GetUser(GetUserRequest request, ServerCallContext context) =>
        (await dispatcher.QueryAsync(
            new GetUserQuery(request.Context.ToUserId()),
            context.CancellationToken)).ToContract();
}
