using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Organizations;

public sealed class OrganizationGroupMember : Entity
{
    private OrganizationGroupMember()
    {
    }

    internal OrganizationGroupMember(Guid groupId, Guid memberId)
    {
        GroupId = groupId;
        MemberId = memberId;
    }

    public Guid GroupId { get; private set; }

    public Guid MemberId { get; private set; }
}

