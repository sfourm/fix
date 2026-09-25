using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Rules;

/// <summary>Role concedida por uma rule.</summary>
public sealed class RuleRole : Entity
{
    private RuleRole()
    {
    }

    internal RuleRole(Guid ruleId, Guid roleId)
    {
        RuleId = ruleId;
        RoleId = roleId;
    }

    public Guid RuleId { get; private set; }

    public Guid RoleId { get; private set; }
}

