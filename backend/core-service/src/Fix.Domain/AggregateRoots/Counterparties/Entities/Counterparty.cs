using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Counterparties;

/// <summary>
/// Contraparte da companhia (trading, banco, corretora, transportadora, produtor). Só contrapartes
/// homologadas recebem novas operações; os limites de nocional e de MTM alimentam o enquadramento.
/// </summary>
public sealed class Counterparty : AggregateRoot, IOrganizationScoped
{
    private Counterparty()
    {
    }

    private Counterparty(Guid organizationId, int number)
    {
        OrganizationId = organizationId;
        Number = number;
        IsHomologated = true;
    }

    public Guid OrganizationId { get; private set; }

    /// <summary>Sequencial na organização; o código legível é CP-01 (I-10).</summary>
    public int Number { get; private set; }

    public string Code => EntityCodes.Counterparty(Number);

    public Name Name { get; private set; } = null!;

    public CounterpartyType Type { get; private set; }

    /// <summary>CNPJ/CPF ou identificador estrangeiro.</summary>
    public string? Document { get; private set; }

    public string? Address { get; private set; }

    /// <summary>País (ISO 3166-1 alfa-2, ex.: BR, CH, US).</summary>
    public string? Country { get; private set; }

    public bool IsHomologated { get; private set; }

    /// <summary>Limite de nocional em US$.</summary>
    public decimal? NotionalLimitUsd { get; private set; }

    /// <summary>Limite de MTM a receber em US$.</summary>
    public decimal? MtmLimitUsd { get; private set; }

    public static Counterparty Create(
        Guid organizationId,
        int number,
        Name name,
        CounterpartyType type,
        string? document,
        string? address,
        string? country,
        decimal? notionalLimitUsd,
        decimal? mtmLimitUsd)
    {
        var counterparty = new Counterparty(organizationId, number);
        counterparty.Update(name, type, document, address, country, notionalLimitUsd, mtmLimitUsd);
        return counterparty;
    }

    public void Update(
        Name name,
        CounterpartyType type,
        string? document,
        string? address,
        string? country,
        decimal? notionalLimitUsd,
        decimal? mtmLimitUsd)
    {
        if (notionalLimitUsd < 0 || mtmLimitUsd < 0)
        {
            throw new DomainException("Os limites da contraparte não podem ser negativos.");
        }

        var countryCode = DomainGuard.OptionalText(country, 2, "país")?.ToUpperInvariant();
        if (countryCode is { Length: not 2 })
        {
            throw new DomainException("O país deve ser um código de 2 letras (ex.: BR).");
        }

        Name = name;
        Type = type;
        Document = DomainGuard.OptionalText(document, 32, "documento");
        Address = DomainGuard.OptionalText(address, 200, "endereço");
        Country = countryCode;
        NotionalLimitUsd = notionalLimitUsd;
        MtmLimitUsd = mtmLimitUsd;
    }

    /// <summary>Homologar libera novas operações; desomologar bloqueia sem apagar o histórico.</summary>
    public void SetHomologation(bool homologated) => IsHomologated = homologated;
}

