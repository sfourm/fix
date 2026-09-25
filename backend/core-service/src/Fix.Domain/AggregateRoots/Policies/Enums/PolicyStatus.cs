namespace Fix.Domain.AggregateRoots.Policies;

public enum PolicyStatus
{
    /// <summary>Rascunho em discussão (editável).</summary>
    Draft = 1,

    /// <summary>Submetida ao Conselho (editável até a ata).</summary>
    UnderApproval = 2,

    /// <summary>Vigente: aprovada com ata; só muda abrindo nova versão.</summary>
    Active = 3,

    /// <summary>Substituída por outra política vigente.</summary>
    Superseded = 4,
}

