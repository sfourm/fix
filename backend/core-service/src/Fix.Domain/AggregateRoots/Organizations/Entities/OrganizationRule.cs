using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Organizations;

/// <summary>
/// Atribuição de uma rule dentro da organização, a um membro OU a um grupo.
/// </summary>
public sealed class OrganizationRule : Entity
{
    private OrganizationRule()
    {
    }

    private OrganizationRule(Guid organizationId, Guid ruleId, Guid? memberId, Guid? groupId)
    {
        OrganizationId = organizationId;
        RuleId = ruleId;
        MemberId = memberId;
        GroupId = groupId;
    }

    public Guid OrganizationId { get; private set; }

    public Guid RuleId { get; private set; }

    public Guid? MemberId { get; private set; }

    public Guid? GroupId { get; private set; }

    internal static OrganizationRule ForMember(Guid organizationId, Guid ruleId, Guid memberId) =>
        new(organizationId, ruleId, memberId, groupId: null);

    internal static OrganizationRule ForGroup(Guid organizationId, Guid ruleId, Guid groupId) =>
        new(organizationId, ruleId, memberId: null, groupId);
}

