namespace Fix.Domain.AggregateRoots.Policies;

public enum InstrumentPermission
{
    Allowed = 1,

    /// <summary>Permitido com teto/condição (ex.: venda coberta de opções até 15% do disponível).</summary>
    Capped = 2,

    Forbidden = 3,
}

