using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Organizations;

public sealed class OrganizationMember : Entity
{
    private OrganizationMember()
    {
    }

    internal OrganizationMember(Guid organizationId, Guid userId, Desk? desk)
    {
        OrganizationId = organizationId;
        UserId = userId;
        Desk = desk;
    }

    public Guid OrganizationId { get; private set; }

    /// <summary>Id do usuário no Identity.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Instância na companhia (mesa, comercial, logística, diretoria, controle de riscos).</summary>
    public Desk? Desk { get; private set; }

    internal void ChangeDesk(Desk? desk) => Desk = desk;
}

