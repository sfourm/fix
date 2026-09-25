using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Organizations;

/// <summary>Grupo do organograma da organização: cada grupo (exceto a raiz) está abaixo de um grupo pai.</summary>
public sealed class OrganizationGroup : Entity
{
    private readonly List<OrganizationGroupMember> _members = [];

    private OrganizationGroup()
    {
    }

    internal OrganizationGroup(Guid organizationId, Name name, bool isDefault, Guid? parentGroupId)
    {
        OrganizationId = organizationId;
        Name = name;
        IsDefault = isDefault;
        ParentGroupId = parentGroupId;
    }

    public Guid OrganizationId { get; private set; }

    public Name Name { get; private set; } = null!;

    public bool IsDefault { get; private set; }

    /// <summary>Grupo imediatamente acima no organograma; null só na raiz.</summary>
    public Guid? ParentGroupId { get; private set; }

    public IReadOnlyCollection<OrganizationGroupMember> Members => _members.AsReadOnly();

    internal void AddMember(Guid memberId)
    {
        if (_members.Any(m => m.MemberId == memberId))
        {
            return;
        }

        _members.Add(new OrganizationGroupMember(Id, memberId));
    }

    internal void MoveUnder(Guid parentGroupId) => ParentGroupId = parentGroupId;

    internal void RemoveMember(Guid memberId) => _members.RemoveAll(m => m.MemberId == memberId);
}

