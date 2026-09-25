using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Rules;

/// <summary>Rule: conjunto de roles (ex.: administrador, gestor, operador) atribuído a membros e grupos.</summary>
public sealed class Rule : AggregateRoot
{
    private readonly List<RuleRole> _roles = [];

    private Rule()
    {
    }

    public Rule(Guid id, string code, string name) : base(id)
    {
        Code = code;
        Name = name;
    }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public IReadOnlyCollection<RuleRole> Roles => _roles.AsReadOnly();

    public void Grant(Guid roleId)
    {
        if (_roles.Any(r => r.RoleId == roleId))
        {
            return;
        }

        _roles.Add(new RuleRole(Id, roleId));
    }
}

