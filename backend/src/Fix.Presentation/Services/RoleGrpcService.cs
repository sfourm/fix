using Fix.Application.Abstractions.Messaging;
using Fix.Application.Roles.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Fix.Presentation.Services;

internal sealed class RoleGrpcService(IDispatcher dispatcher) : RoleService.RoleServiceBase
{
    public override async Task<ListRolesResponse> ListRoles(Empty request, ServerCallContext context)
    {
        var roles = await dispatcher.QueryAsync(new ListRolesQuery(), context.CancellationToken);
        return new ListRolesResponse { Roles = { roles.Select(r => r.ToContract()) } };
    }
}
