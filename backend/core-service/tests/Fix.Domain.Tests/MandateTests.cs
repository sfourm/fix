using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Policies;
using Fix.Domain.Services;

namespace Fix.Domain.Tests;

public sealed class MandateTests
{
    [Fact]
    public void Compliance_is_within_for_active_policy_and_tenor_inside_horizon()
    {
        var policy = TestData.ActivePolicy();

        var compliance = MandateCompliance.Evaluate(policy, TestData.CreateBudget(), MandateType.Pricing, TestData.PricingTerms(), TestData.Today);

        Assert.True(compliance.IsWithin);
    }

    [Fact]
    public void Tenor_beyond_hedge_horizon_is_outside()
    {
        var policy = TestData.ActivePolicy();

        var compliance = MandateCompliance.Evaluate(
            policy, TestData.CreateBudget(), MandateType.Pricing, TestData.PricingTerms(tenor: "K29"), TestData.Today);

        Assert.Equal(ComplianceStatus.Outside, compliance.Status);
        Assert.Contains("horizonte", compliance.Reason);
    }

    [Fact]
    public void Minimum_price_below_economic_floor_is_outside()
    {
        var policy = TestData.ActivePolicy();

        var compliance = MandateCompliance.Evaluate(
            policy, TestData.CreateBudget(), MandateType.Pricing, TestData.PricingTerms(priceMin: 14.5m), TestData.Today);

        Assert.Equal(ComplianceStatus.Outside, compliance.Status);
        Assert.Contains("piso econômico", compliance.Reason);
    }

    [Fact]
    public void Policy_not_in_force_makes_mandate_outside()
    {
        var policy = TestData.ActivePolicy();
        policy.OpenNewVersion("v1.1", null, TestData.Today);

        var compliance = MandateCompliance.Evaluate(policy, TestData.CreateBudget(), MandateType.Pricing, TestData.PricingTerms(), TestData.Today);

        Assert.Equal(ComplianceStatus.Outside, compliance.Status);
    }

    [Theory]
    [InlineData(true, true, MandateStatus.Active)]
    [InlineData(true, false, MandateStatus.PendingApproval)]
    [InlineData(false, true, MandateStatus.PendingApproval)]
    public void Issue_is_active_only_when_within_and_issuer_has_authority(bool within, bool authority, MandateStatus expected)
    {
        var policy = TestData.ActivePolicy();

        var mandate = Mandate.Issue(
            policy,
            TestData.AxisOf(policy, RiskFactor.Price),
            MandateType.Pricing,
            TestData.PricingTerms(),
            within ? Compliance.Within("ok") : Compliance.Outside("fora"),
            TestData.UserId,
            authority);

        Assert.Equal(expected, mandate.Status);
    }

    [Fact]
    public void Axis_must_cover_the_mandate_factor()
    {
        var policy = TestData.ActivePolicy();

        Assert.Throws<DomainException>(() => Mandate.Issue(
            policy,
            TestData.AxisOf(policy, RiskFactor.Currency),
            MandateType.Pricing,
            TestData.PricingTerms(),
            Compliance.Within("ok"),
            TestData.UserId,
            issuerHasAuthority: true));
    }

    [Fact]
    public void Rejection_requires_justification_and_only_pending_can_be_decided()
    {
        var policy = TestData.ActivePolicy();
        var mandate = Mandate.Issue(
            policy, TestData.AxisOf(policy, RiskFactor.Price), MandateType.Pricing, TestData.PricingTerms(),
            Compliance.Outside("fora"), TestData.UserId, issuerHasAuthority: false);

        Assert.Throws<DomainException>(() => mandate.Reject(TestData.UserId, DateTimeOffset.UtcNow, " "));

        mandate.Approve(TestData.UserId, DateTimeOffset.UtcNow, null);
        Assert.Equal(MandateStatus.Active, mandate.Status);
        Assert.Throws<DomainException>(() => mandate.Approve(TestData.UserId, DateTimeOffset.UtcNow, null));
    }

    [Fact]
    public void Mandate_without_quantity_needs_a_price_criteria()
    {
        var policy = TestData.ActivePolicy();
        var terms = TestData.PricingTerms(lots: null) with { Price = PriceCriteria.None() };

        Assert.Throws<DomainException>(() => Mandate.Issue(
            policy, TestData.AxisOf(policy, RiskFactor.Price), MandateType.Pricing, terms,
            Compliance.Within("ok"), TestData.UserId, issuerHasAuthority: true));
    }
}
