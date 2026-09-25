namespace Fix.Domain.Common;

public enum ComplianceStatus
{
    /// <summary>Dentro da política.</summary>
    Within = 1,

    /// <summary>FORA da política: exige aprovação por alçada de exceção.</summary>
    Outside = 2,
}
