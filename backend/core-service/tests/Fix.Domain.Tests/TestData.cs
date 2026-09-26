using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Counterparties;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Domain.Tests;

/// <summary>Montagem de agregados válidos para os testes (hoje = 14/08/2026, safra 26/27).</summary>
internal static class TestData
{
    public static readonly DateOnly Today = new(2026, 8, 14);
    public static readonly Guid OrganizationId = Guid.NewGuid();
    public static readonly Guid UserId = Guid.NewGuid();

    /// <summary>Middle office: outra pessoa, que confere o confirmation (segregação).</summary>
    public static readonly Guid MiddleOfficeId = Guid.NewGuid();

    public static Policy ActivePolicy()
    {
        var policy = Policy.Create(
            OrganizationId,
            "POL-2026",
            Title.Create("Política de riscos"),
            "v1.0",
            null,
            DateRange.Create(new DateOnly(2026, 1, 1), new DateOnly(2027, 12, 31)),
            Today);
        PolicyTemplate.Apply(policy, CropYear.Create("26/27"));
        policy.Submit(Today);
        policy.Approve("ata 01/2026", Today);
        return policy;
    }

    public static Guid AxisOf(Policy policy, RiskFactor factor) => policy.Axes.First(a => a.Factor == factor).Id;

    public static Budget CreateBudget() => Budget.Create(cashCost: 14.2m, economicFloor: 15.0m, equivalentPrice: 16m, targetMarginPct: 12);

    public static MandateTerms PricingTerms(string tenor = "N26", decimal? lots = 800, decimal? priceMin = null) => new(
        Title.Create("Fixar 30% da tela"),
        null,
        Commodity.RawSugar,
        Tenor.Create(tenor),
        lots,
        lots is null ? null : MeasurementUnit.Lots,
        PriceCriteria.Create(false, null, priceMin, null, "c/lb"),
        null,
        null);

    public static Mandate ActivePricingMandate(decimal? lots = 800)
    {
        var policy = ActivePolicy();
        return Mandate.Issue(
            1,
            policy,
            AxisOf(policy, RiskFactor.Price),
            MandateType.Pricing,
            PricingTerms(lots: lots),
            Compliance.Within("dentro"),
            UserId,
            issuerHasAuthority: true);
    }

    public static Counterparty CreateCounterparty(bool homologated = true)
    {
        var counterparty = Counterparty.Create(
            OrganizationId,
            1,
            Name.Create("Louis Dreyfus Company"),
            CounterpartyType.Trading,
            null,
            null,
            "CH",
            10_000_000,
            1_500_000);
        counterparty.SetHomologation(homologated);
        return counterparty;
    }
}
