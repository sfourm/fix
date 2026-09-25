using Fix.Application.Abstractions.Validation;
using Fix.Application.Auth.Commands;
using Fix.Application.Auth.Queries;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Fix.Presentation.UseCases;
using Grpc.Core;

namespace Fix.Presentation.Services;

internal sealed class AuthGrpcService(
    IValidationFactory validation,
    IAuthService authService) : AuthService.AuthServiceBase
{
    public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        var userId = await validation.RunAsync(
            new RegisterUserCommand(request.Email, request.Password, request.FullName),
            authService.RegisterUserAsync,
            context.CancellationToken);

        return new RegisterResponse { UserId = userId.ToString() };
    }

    public override async Task<User> Authenticate(AuthenticateRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new AuthenticateUserCommand(request.Email, request.Password),
            authService.AuthenticateUserAsync,
            context.CancellationToken)).ToContract();

    public override async Task<User> GetUser(GetUserRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new GetUserQuery(request.Context.ToUserId()),
            authService.GetUserAsync,
            context.CancellationToken)).ToContract();
}
