using Fix.Application.Abstractions.Validation;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Roles.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Fix.Presentation.UseCases;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Fix.Presentation.Services;

internal sealed class RoleGrpcService(
    IValidationFactory validation,
    IRoleService roleService) : RoleService.RoleServiceBase
{
    public override async Task<ListRolesResponse> ListRoles(Empty request, ServerCallContext context)
    {
        var roles = await validation.RunAsync(new ListRolesQuery(), roleService.ListRolesAsync, context.CancellationToken);
        return new ListRolesResponse { Roles = { roles.Select(r => r.ToContract()) } };
    }
}
