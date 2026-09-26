using Fix.Storage.Application.Abstractions.Authorization;
using Fix.Storage.Application.Abstractions.Exceptions;
using Fix.Storage.Domain.AggregateRoots.Files;

namespace Fix.Storage.Application.Authorization;

/// <summary>
/// Quem pode enviar e ver cada tipo de arquivo, pelas regras efetivas do usuário no core (cargos diretos e dos grupos).
/// Documentos: qualquer membro da organização.
/// </summary>
internal sealed class FilePermissions(IRoleResolver roleResolver)
{
    private static readonly Dictionary<FileKind, (string? Upload, string? View)> Rules = new()
    {
        [FileKind.Users] = ("create_user", "view_user"),
        [FileKind.Mandates] = ("create_mandate", "view_mandate"),
        [FileKind.Orders] = ("create_order", "view_order"),
        // Só criação de políticas: exige create_policy (alterações seguem pela tela).
        [FileKind.Policies] = ("create_policy", "view_policy"),
        [FileKind.Documents] = (null, null),
    };

    public Task<IReadOnlySet<string>> GetRolesAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken) =>
        roleResolver.GetRolesAsync(organizationId, userId, cancellationToken);

    public static bool CanUpload(IReadOnlySet<string> roles, FileKind kind) => Rules[kind].Upload is not { } role || roles.Contains(role);

    public static bool CanView(IReadOnlySet<string> roles, FileKind kind) => Rules[kind].View is not { } role || roles.Contains(role);

    public static void EnsureUpload(IReadOnlySet<string> roles, FileKind kind)
    {
        if (!CanUpload(roles, kind))
        {
            throw new ForbiddenException($"Enviar arquivo de {kind.Label()} exige a regra {Rules[kind].Upload}.");
        }
    }

    public static void EnsureView(IReadOnlySet<string> roles, FileKind kind)
    {
        if (!CanView(roles, kind))
        {
            throw new ForbiddenException($"Ver arquivos de {kind.Label()} exige a regra {Rules[kind].View}.");
        }
    }
}
