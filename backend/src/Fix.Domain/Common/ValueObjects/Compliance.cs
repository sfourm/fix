using Fix.Domain.Abstractions;

namespace Fix.Domain.Common;

/// <summary>Enquadramento (dentro/FORA da política) com o motivo legível.</summary>
public sealed class Compliance : ValueObject
{
    private Compliance()
    {
    }

    private Compliance(ComplianceStatus status, string reason)
    {
        Status = status;
        Reason = reason;
    }

    public ComplianceStatus Status { get; private set; }

    public string Reason { get; private set; } = null!;

    public bool IsWithin => Status == ComplianceStatus.Within;

    public static Compliance Within(string reason) => new(ComplianceStatus.Within, reason);

    public static Compliance Outside(string reason) => new(ComplianceStatus.Outside, reason);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Status;
        yield return Reason;
    }
}
