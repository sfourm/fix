using Fix.Domain.Abstractions;

namespace Fix.Domain.Common;

public static class Percentage
{
    /// <summary>Garante um percentual entre 0 e o máximo informado (padrão 100).</summary>
    public static decimal Ensure(decimal value, string field, decimal max = 100)
    {
        if (value < 0 || value > max)
        {
            throw new DomainException($"'{field}' deve estar entre 0 e {max}%.");
        }

        return value;
    }

    public static decimal? EnsureOptional(decimal? value, string field, decimal max = 100) =>
        value is null ? null : Ensure(value.Value, field, max);

    public static void EnsureRange(decimal? min, decimal? max, string field)
    {
        if (min is not null && max is not null && min > max)
        {
            throw new DomainException($"'{field}': o mínimo não pode ser maior que o máximo.");
        }
    }
}
