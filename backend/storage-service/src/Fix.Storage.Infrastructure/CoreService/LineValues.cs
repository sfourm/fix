using System.Globalization;
using Fix.Contracts.V1;
using Fix.Storage.Domain.AggregateRoots.Files.Templates;

namespace Fix.Storage.Infrastructure.CoreService;

/// <summary>Valor de uma linha fora do formato esperado (vira falha da linha, com a mensagem para o usuário).</summary>
internal sealed class LineValueException(string message) : Exception(message);

/// <summary>
/// Leitura dos valores de uma linha como o usuário digita no Excel em pt-BR: números "16,42" (ou "16.42"), datas
/// dd/mm/aaaa (ou aaaa-mm-dd), sim/não e opções sem acento ("logística" = "logistica").
/// </summary>
internal sealed class LineValues(IReadOnlyDictionary<string, string> values)
{
    private static readonly string[] DateFormats = ["dd/MM/yyyy", "d/M/yyyy", "dd/MM/yy", "yyyy-MM-dd", "dd-MM-yyyy", "dd.MM.yyyy"];

    public string? Text(string column) =>
        values.TryGetValue(column, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;

    public string Required(string column) => Text(column) ?? throw new LineValueException($"Preencha a coluna {column}.");

    public double? Number(string column)
    {
        if (Text(column) is not { } text)
        {
            return null;
        }

        return ParseNumber(text) ?? throw new LineValueException($"A coluna {column} precisa ser um número (recebido: \"{text}\").");
    }

    public double RequiredNumber(string column) => Number(column) ?? throw new LineValueException($"Preencha a coluna {column}.");

    /// <summary>Data no formato do contrato (aaaa-mm-dd).</summary>
    public string? Date(string column)
    {
        if (Text(column) is not { } text)
        {
            return null;
        }

        return DateOnly.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : throw new LineValueException($"A coluna {column} precisa ser uma data dd/mm/aaaa (recebido: \"{text}\").");
    }

    public string RequiredDate(string column) => Date(column) ?? throw new LineValueException($"Preencha a coluna {column}.");

    public bool Flag(string column)
    {
        if (Text(column) is not { } text)
        {
            return false;
        }

        return FileTemplates.Normalize(text) switch
        {
            "sim" or "s" or "yes" or "y" or "true" or "verdadeiro" or "1" or "x" => true,
            "nao" or "n" or "no" or "false" or "falso" or "0" => false,
            _ => throw new LineValueException($"A coluna {column} aceita sim ou não (recebido: \"{text}\")."),
        };
    }

    /// <summary>Opção de uma lista: compara sem acento e sem diferença de maiúsculas.</summary>
    public T? Option<T>(string column, IReadOnlyDictionary<string, T> options)
        where T : struct
    {
        if (Text(column) is not { } text)
        {
            return null;
        }

        return options.TryGetValue(FileTemplates.Normalize(text), out var option)
            ? option
            : throw new LineValueException(
                $"A coluna {column} aceita {string.Join(", ", options.Keys.Distinct().Take(8))} (recebido: \"{text}\").");
    }

    public T RequiredOption<T>(string column, IReadOnlyDictionary<string, T> options)
        where T : struct => Option(column, options) ?? throw new LineValueException($"Preencha a coluna {column}.");

    /// <summary>"1.234,56", "1234,56", "1234.56" e "1,234.56": o último separador é o decimal.</summary>
    public static double? ParseNumber(string text)
    {
        var value = text.Replace(" ", string.Empty).Replace(" ", string.Empty);
        var comma = value.LastIndexOf(',');
        var dot = value.LastIndexOf('.');
        if (comma >= 0 && dot >= 0)
        {
            value = comma > dot ? value.Replace(".", string.Empty).Replace(',', '.') : value.Replace(",", string.Empty);
        }
        else if (comma >= 0)
        {
            value = value.Count(c => c == ',') > 1 ? value.Replace(",", string.Empty) : value.Replace(',', '.');
        }
        else if (value.Count(c => c == '.') > 1)
        {
            value = value.Replace(".", string.Empty);
        }

        return double.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number)
            ? number
            : null;
    }

    public static readonly IReadOnlyDictionary<string, Desk> Desks = new Dictionary<string, Desk>
    {
        ["execucao"] = Desk.ExecutionDesk,
        ["mesa"] = Desk.ExecutionDesk,
        ["mesa_de_execucao"] = Desk.ExecutionDesk,
        ["comercial"] = Desk.Commercial,
        ["logistica"] = Desk.Logistics,
        ["diretoria"] = Desk.Board,
        ["controle_riscos"] = Desk.RiskControl,
        ["controle_de_riscos"] = Desk.RiskControl,
        ["riscos"] = Desk.RiskControl,
    };

    public static readonly IReadOnlyDictionary<string, MandateType> MandateTypes = new Dictionary<string, MandateType>
    {
        ["precificacao"] = MandateType.Pricing,
        ["preco"] = MandateType.Pricing,
        ["fixacao"] = MandateType.Pricing,
        ["moeda"] = MandateType.Currency,
        ["cambio"] = MandateType.Currency,
        ["comercial"] = MandateType.Commercial,
        ["fisico"] = MandateType.Commercial,
        ["logistica"] = MandateType.Logistics,
        ["frete"] = MandateType.Logistics,
    };

    public static readonly IReadOnlyDictionary<string, Commodity> Commodities = new Dictionary<string, Commodity>
    {
        ["acucar_vhp"] = Commodity.RawSugar,
        ["vhp"] = Commodity.RawSugar,
        ["ny11"] = Commodity.RawSugar,
        ["acucar_branco"] = Commodity.WhiteSugar,
        ["branco"] = Commodity.WhiteSugar,
        ["etanol_hidratado"] = Commodity.HydratedEthanol,
        ["hidratado"] = Commodity.HydratedEthanol,
        ["etanol_anidro"] = Commodity.AnhydrousEthanol,
        ["anidro"] = Commodity.AnhydrousEthanol,
        ["milho"] = Commodity.Corn,
        ["soja"] = Commodity.Soybean,
    };

    public static readonly IReadOnlyDictionary<string, MeasurementUnit> Units = new Dictionary<string, MeasurementUnit>
    {
        ["lotes"] = MeasurementUnit.Lots,
        ["lote"] = MeasurementUnit.Lots,
        ["toneladas"] = MeasurementUnit.Tonnes,
        ["t"] = MeasurementUnit.Tonnes,
        ["sacas"] = MeasurementUnit.Bags,
        ["sc"] = MeasurementUnit.Bags,
        ["m3"] = MeasurementUnit.CubicMeters,
        ["metros_cubicos"] = MeasurementUnit.CubicMeters,
        ["libras"] = MeasurementUnit.Pounds,
        ["lb"] = MeasurementUnit.Pounds,
        ["usd"] = MeasurementUnit.Usd,
        ["us"] = MeasurementUnit.Usd,
    };

    public static readonly IReadOnlyDictionary<string, OrderType> OrderTypes = new Dictionary<string, OrderType>
    {
        ["futuro"] = OrderType.Futures,
        ["futuros"] = OrderType.Futures,
        ["opcao"] = OrderType.Option,
        ["opcoes"] = OrderType.Option,
        ["ndf"] = OrderType.Ndf,
    };

    public static readonly IReadOnlyDictionary<string, TradeDirection> Directions = new Dictionary<string, TradeDirection>
    {
        ["compra"] = TradeDirection.Buy,
        ["c"] = TradeDirection.Buy,
        ["venda"] = TradeDirection.Sell,
        ["v"] = TradeDirection.Sell,
    };

    public static readonly IReadOnlyDictionary<string, OptionKind> OptionKinds = new Dictionary<string, OptionKind>
    {
        ["call"] = OptionKind.Call,
        ["put"] = OptionKind.Put,
    };
}
