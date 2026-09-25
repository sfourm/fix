using System.Text.RegularExpressions;
using Fix.Domain.Abstractions;

namespace Fix.Domain.Common;

/// <summary>Ano-safra no formato "26/27" (a safra começa no mês configurado pela companhia).</summary>
public sealed partial class CropYear : ValueObject
{
    private CropYear(string value, int startYear)
    {
        Value = value;
        StartYear = startYear;
    }

    public string Value { get; }

    public int StartYear { get; }

    public static CropYear Create(string? value)
    {
        var match = Pattern().Match(value?.Trim() ?? string.Empty);
        if (!match.Success)
        {
            throw new DomainException("Safra deve estar no formato AA/AA (ex.: 26/27).");
        }

        var first = int.Parse(match.Groups[1].Value);
        var second = int.Parse(match.Groups[2].Value);
        if ((first + 1) % 100 != second)
        {
            throw new DomainException("Safra deve cobrir anos consecutivos (ex.: 26/27).");
        }

        return new CropYear($"{match.Groups[1].Value}/{match.Groups[2].Value}", 2000 + first);
    }

    /// <summary>Safra que contém a data, dado o mês de início do ano-safra (ex.: 4 = abril).</summary>
    public static CropYear Of(DateOnly date, int startMonth)
    {
        var start = date.Month >= startMonth ? date.Year : date.Year - 1;
        return Create($"{start % 100:00}/{(start + 1) % 100:00}");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^(\d{2})/(\d{2})$")]
    private static partial Regex Pattern();
}
