using System.Globalization;

namespace Fix.Domain.Common;

/// <summary>
/// Identificadores legíveis por prefixo da entidade (FIX2 · I-10): MD-01 (mandato), HX-0001 (boleta), CP-01 (contraparte).
/// O número é sequencial por organização; o id técnico continua sendo o UUID.
/// </summary>
public static class EntityCodes
{
    public static string Mandate(int number) => Format("MD", number, 2);

    public static string Order(int number) => Format("HX", number, 4);

    public static string Counterparty(int number) => Format("CP", number, 2);

    private static string Format(string prefix, int number, int digits) =>
        $"{prefix}-{number.ToString(new string('0', digits), CultureInfo.InvariantCulture)}";
}
