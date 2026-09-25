using Fix.Domain.Abstractions;

namespace Fix.Domain.AggregateRoots.Policies;

/// <summary>Registro do histórico de versões da política (rascunho, submissão, vigência, revisões).</summary>
public sealed class PolicyVersion : Entity
{
    private PolicyVersion()
    {
    }

    internal PolicyVersion(Guid policyId, string version, PolicyStatus status, DateOnly date, string? note)
    {
        PolicyId = policyId;
        Version = version;
        Status = status;
        Date = date;
        Note = note;
    }

    public Guid PolicyId { get; private set; }

    public string Version { get; private set; } = null!;

    public PolicyStatus Status { get; private set; }

    public DateOnly Date { get; private set; }

    public string? Note { get; private set; }
}

