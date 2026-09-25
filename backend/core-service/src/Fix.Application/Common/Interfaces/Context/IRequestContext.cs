namespace Fix.Application.Abstractions.Context;

/// <summary>
/// Usuário e organização da requisição em andamento. Preenchido pelo pipeline a partir do
/// command/query e usado pela infraestrutura (filtro de tenant do EF e autor da auditoria).
/// </summary>
public interface IRequestContext
{
    Guid? UserId { get; }

    Guid? OrganizationId { get; }
}

internal sealed class RequestContext : IRequestContext
{
    public Guid? UserId { get; private set; }

    public Guid? OrganizationId { get; private set; }

    public void Set(Guid userId, Guid? organizationId)
    {
        UserId = userId;
        OrganizationId = organizationId;
    }
}
