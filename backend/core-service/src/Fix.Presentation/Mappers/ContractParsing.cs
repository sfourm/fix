using System.Globalization;
using Fix.Application.Abstractions.Exceptions;
using Fix.Contracts.V1;

namespace Fix.Presentation.Mappers;

/// <summary>
/// Conversões dos tipos primitivos do contrato: ids e datas em string, decimais em double
/// e enums com os mesmos valores numéricos do domínio (0 = não informado).
/// </summary>
internal static class ContractParsing
{
    private const string DateFormat = "yyyy-MM-dd";

    public static Guid ToUserId(this RequestContext? context) =>
        (context?.UserId ?? string.Empty).ToGuid("context.user_id");

    public static Guid ToOrganizationId(this RequestContext? context) =>
        (context?.OrganizationId ?? string.Empty).ToGuid("context.organization_id");

    public static Guid ToGuid(this string value, string field) =>
        Guid.TryParse(value, out var id) ? id : throw new BadRequestException($"'{field}' deve ser um UUID válido.");

    public static Guid? ToOptionalGuid(this string? value, string field) =>
        string.IsNullOrWhiteSpace(value) ? null : value.ToGuid(field);

    // ---------- Datas ----------

    public static DateOnly ToDate(this string value, string field) =>
        DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : throw new BadRequestException($"'{field}' deve estar no formato {DateFormat}.");

    public static DateOnly? ToOptionalDate(this string? value, string field) =>
        string.IsNullOrWhiteSpace(value) ? null : value.ToDate(field);

    /// <summary>Instante ISO 8601 (com fuso; sem fuso vale UTC).</summary>
    public static DateTimeOffset? ToOptionalMoment(this string? value, string field) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var moment)
                ? moment.ToUniversalTime()
                : throw new BadRequestException($"'{field}' deve ser uma data/hora ISO 8601.");

    public static string ToContract(this DateOnly date) => date.ToString(DateFormat, CultureInfo.InvariantCulture);

    public static string ToContract(this DateTimeOffset moment) => moment.ToString("O", CultureInfo.InvariantCulture);

    // ---------- Decimais e opcionais ----------

    public static decimal ToDecimal(this double value) => (decimal)value;

    /// <summary>Campo proto3 optional: só vale se "has" for verdadeiro.</summary>
    public static decimal? ToOptionalDecimal(this double value, bool has) => has ? (decimal)value : null;

    public static string? ToOptionalString(this string value, bool has) =>
        has && !string.IsNullOrWhiteSpace(value) ? value : null;

    // ---------- Enums ----------

    public static TDomain ToDomain<TDomain>(this Enum value, string field)
        where TDomain : struct, Enum =>
        value.ToOptionalDomain<TDomain>(field) ?? throw new BadRequestException($"'{field}' é obrigatório.");

    public static TDomain? ToOptionalDomain<TDomain>(this Enum value, string field)
        where TDomain : struct, Enum
    {
        var number = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        if (number == 0)
        {
            return null;
        }

        return Enum.IsDefined(typeof(TDomain), number)
            ? (TDomain)Enum.ToObject(typeof(TDomain), number)
            : throw new BadRequestException($"'{field}' possui valor inválido.");
    }

    public static TContract ToContract<TContract>(this Enum? value)
        where TContract : struct, Enum =>
        (TContract)Enum.ToObject(typeof(TContract), value is null ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture));

    // ---------- Paginação ----------

    public static (int Page, int PageSize) ToPaging(this PageRequest? page) =>
        (page?.Page ?? 0, page?.PageSize ?? 0);

    public static PageInfo ToPageInfo<T>(this Domain.Abstractions.PagedList<T> list) =>
        new() { Page = list.Page, PageSize = list.PageSize, TotalCount = list.TotalCount };
}
