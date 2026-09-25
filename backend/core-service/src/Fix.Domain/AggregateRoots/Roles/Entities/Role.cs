using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Roles;

/// <summary>Role: permissão atômica do sistema (ex.: create_policy). As alçadas do FIX são roles.</summary>
public sealed class Role : Entity
{
    private Role()
    {
    }

    public Role(Guid id, string code, string description) : base(id)
    {
        Code = code;
        Description = description;
    }

    public string Code { get; private set; } = null!;

    public string Description { get; private set; } = null!;
}

