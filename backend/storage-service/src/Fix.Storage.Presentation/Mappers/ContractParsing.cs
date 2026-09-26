using System.Globalization;
using Fix.Contracts.V1;
using Fix.Storage.Application.Abstractions.Exceptions;

namespace Fix.Storage.Presentation.Mappers;

/// <summary>
/// Conversões dos tipos primitivos do contrato: ids e datas em string e enums com os mesmos valores numéricos do
/// domínio (0 = não informado).
/// </summary>
internal static class ContractParsing
{
    public static Guid ToUserId(this RequestContext? context) =>
        (context?.UserId ?? string.Empty).ToGuid("context.user_id");

    public static Guid ToOrganizationId(this RequestContext? context) =>
        (context?.OrganizationId ?? string.Empty).ToGuid("context.organization_id");

    public static Guid ToGuid(this string value, string field) =>
        Guid.TryParse(value, out var id) ? id : throw new BadRequestException($"'{field}' deve ser um UUID válido.");

    // ---------- Datas ----------

    public static string ToContract(this DateTimeOffset moment) => moment.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

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
