using System.Text.RegularExpressions;
using Fix.Domain.Abstractions;

namespace Fix.Domain.Common;

/// <summary>
/// Vencimento / "tela": código de bolsa (ex.: N26 = julho/2026) ou mês (ex.: fev/27).
/// </summary>
public sealed partial class Tenor : ValueObject
{
    private static readonly Dictionary<char, int> FuturesMonths = new()
    {
        ['F'] = 1, ['G'] = 2, ['H'] = 3, ['J'] = 4, ['K'] = 5, ['M'] = 6,
        ['N'] = 7, ['Q'] = 8, ['U'] = 9, ['V'] = 10, ['X'] = 11, ['Z'] = 12,
    };

    private static readonly string[] MonthNames = ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"];

    private Tenor(string code, DateOnly month)
    {
        Code = code;
        Month = month;
    }

    public string Code { get; }

    /// <summary>Primeiro dia do mês de vencimento.</summary>
    public DateOnly Month { get; }

    public static Tenor Create(string? code)
    {
        var value = code?.Trim() ?? string.Empty;

        var futures = FuturesPattern().Match(value.ToUpperInvariant());
        if (futures.Success)
        {
            var month = FuturesMonths[futures.Groups[1].Value[0]];
            return new Tenor(futures.Value, new DateOnly(2000 + int.Parse(futures.Groups[2].Value), month, 1));
        }

        var named = MonthPattern().Match(value.ToLowerInvariant());
        if (named.Success)
        {
            var month = Array.IndexOf(MonthNames, named.Groups[1].Value) + 1;
            if (month > 0)
            {
                return new Tenor(named.Value, new DateOnly(2000 + int.Parse(named.Groups[2].Value), month, 1));
            }
        }

        throw new DomainException("Vencimento inválido. Use o código de bolsa (ex.: N26) ou mês/ano (ex.: fev/27).");
    }

    /// <summary>Meses entre a data de referência e o vencimento (negativo se já venceu).</summary>
    public int MonthsFrom(DateOnly reference) =>
        ((Month.Year - reference.Year) * 12) + (Month.Month - reference.Month);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString() => Code;

    [GeneratedRegex(@"^([FGHJKMNQUVXZ])(\d{2})$")]
    private static partial Regex FuturesPattern();

    [GeneratedRegex(@"^([a-z]{3})/(\d{2})$")]
    private static partial Regex MonthPattern();
}
