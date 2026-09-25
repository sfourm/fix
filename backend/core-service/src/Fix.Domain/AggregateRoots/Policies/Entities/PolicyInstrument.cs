using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Policies;

/// <summary>Instrumento previsto na política: permitido, com teto ou vedado · §8.</summary>
public sealed class PolicyInstrument : Entity
{
    private PolicyInstrument()
    {
    }

    internal PolicyInstrument(Guid policyId) => PolicyId = policyId;

    public Guid PolicyId { get; private set; }

    public string Name { get; private set; } = null!;

    public InstrumentPermission Permission { get; private set; }

    public string? Condition { get; private set; }

    internal void Update(string name, InstrumentPermission permission, string? condition)
    {
        Name = DomainGuard.OptionalText(name, 150, "instrumento") ?? throw new DomainException("O nome do instrumento é obrigatório.");
        Permission = permission;
        Condition = DomainGuard.OptionalText(condition, 300, "condição");
    }
}

