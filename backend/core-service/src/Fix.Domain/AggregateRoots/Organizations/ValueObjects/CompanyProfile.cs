using System.Text.RegularExpressions;
using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Organizations;

/// <summary>Identificação da empresa e calendário de safra (Setup · configurações gerais).</summary>
public sealed partial class CompanyProfile : ValueObject
{
    // Usado pelo EF Core (complex type).
    private CompanyProfile()
    {
    }

    private CompanyProfile(
        string corporateName,
        string? taxId,
        string? headquarters,
        string? group,
        Sector sector,
        int cropYearStartMonth,
        string? activeCrop)
    {
        CorporateName = corporateName;
        TaxId = taxId;
        Headquarters = headquarters;
        Group = group;
        Sector = sector;
        CropYearStartMonth = cropYearStartMonth;
        ActiveCrop = activeCrop;
    }

    /// <summary>Razão social.</summary>
    public string CorporateName { get; private set; } = null!;

    /// <summary>CNPJ (somente dígitos).</summary>
    public string? TaxId { get; private set; }

    public string? Headquarters { get; private set; }

    public string? Group { get; private set; }

    public Sector Sector { get; private set; }

    /// <summary>Mês de início do ano-safra (sucroenergético: abril = 4).</summary>
    public int CropYearStartMonth { get; private set; }

    /// <summary>Safra ativa no formato AA/AA.</summary>
    public string? ActiveCrop { get; private set; }

    public static CompanyProfile Default(Name name) => new(name.Value, null, null, null, Sector.SugarEnergy, 4, null);

    public static CompanyProfile Create(
        string? corporateName,
        string? taxId,
        string? headquarters,
        string? group,
        Sector sector,
        int cropYearStartMonth,
        string? activeCrop)
    {
        if (string.IsNullOrWhiteSpace(corporateName))
        {
            throw new DomainException("A razão social é obrigatória.");
        }

        if (cropYearStartMonth is < 1 or > 12)
        {
            throw new DomainException("O mês de início do ano-safra deve estar entre 1 e 12.");
        }

        string? digits = null;
        if (!string.IsNullOrWhiteSpace(taxId))
        {
            digits = NonDigits().Replace(taxId, string.Empty);
            if (digits.Length != 14)
            {
                throw new DomainException("O CNPJ deve ter 14 dígitos.");
            }
        }

        var crop = string.IsNullOrWhiteSpace(activeCrop) ? null : CropYear.Create(activeCrop).Value;

        return new CompanyProfile(
            DomainGuard.OptionalText(corporateName, 200, "razão social")!,
            digits,
            DomainGuard.OptionalText(headquarters, 150, "sede"),
            DomainGuard.OptionalText(group, 150, "grupo"),
            sector,
            cropYearStartMonth,
            crop);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CorporateName;
        yield return TaxId;
        yield return Headquarters;
        yield return Group;
        yield return Sector;
        yield return CropYearStartMonth;
        yield return ActiveCrop;
    }

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigits();
}

