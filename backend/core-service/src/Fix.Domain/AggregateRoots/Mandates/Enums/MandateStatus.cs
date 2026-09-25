namespace Fix.Domain.AggregateRoots.Mandates;

public enum MandateStatus
{
    /// <summary>Aguardando aprovação (FORA da política ou emissor sem alçada). Não aceita boletas.</summary>
    PendingApproval = 1,

    /// <summary>Ativo: autoriza boletas até o saldo.</summary>
    Active = 2,

    Rejected = 3,

    /// <summary>Encerrado: não aceita novas boletas.</summary>
    Closed = 4,
}

