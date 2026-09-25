using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Domain.Tests;

public sealed class PolicyTests
{
    private static Policy Draft() => Policy.Create(
        TestData.OrganizationId,
        "pol-2026",
        Title.Create("Política de riscos"),
        "v1.0",
        null,
        DateRange.Create(new DateOnly(2026, 1, 1), null),
        TestData.Today);

    [Fact]
    public void Create_starts_as_draft_with_default_limits_and_version_history()
    {
        var policy = Draft();

        Assert.Equal(PolicyStatus.Draft, policy.Status);
        Assert.Equal("POL-2026", policy.Code);
        Assert.Equal(2, policy.Limits.HedgeHorizonYears);
        Assert.Single(policy.Versions);
    }

    [Fact]
    public void Template_brings_axes_instruments_and_bands_for_three_crops()
    {
        var policy = Draft();

        PolicyTemplate.Apply(policy, CropYear.Create("26/27"));

        Assert.Equal(5, policy.Axes.Count);
        Assert.Contains(policy.Instruments, i => i.Permission == InstrumentPermission.Forbidden);
        Assert.Equal(["26/27", "27/28", "28/29"], policy.Bands.Select(b => b.Crop.Value));
    }

    [Fact]
    public void Lifecycle_requires_axes_and_approval_record()
    {
        var policy = Draft();

        Assert.Throws<DomainException>(() => policy.Submit(TestData.Today));

        PolicyTemplate.Apply(policy, null);
        policy.Submit(TestData.Today);
        Assert.Throws<DomainException>(() => policy.Approve(" ", TestData.Today));

        policy.Approve("ata 01/2026", TestData.Today);
        Assert.Equal(PolicyStatus.Active, policy.Status);
        Assert.True(policy.IsInForceOn(TestData.Today));
    }

    [Fact]
    public void Active_policy_is_read_only_until_a_new_version_is_opened()
    {
        var policy = TestData.ActivePolicy();

        Assert.Throws<DomainException>(() => policy.UpdateLimits(PolicyLimits.Default()));

        policy.OpenNewVersion("v1.1", "revisão das bandas", TestData.Today);
        Assert.Equal(PolicyStatus.UnderApproval, policy.Status);
        Assert.Null(policy.ApprovalRecord);
        policy.UpdateLimits(PolicyLimits.Default());
    }

    [Fact]
    public void Band_limits_and_duplicated_crop_are_rejected()
    {
        var policy = Draft();
        policy.AddBand("Safra corrente", CropYear.Create("26/27"), 60, 120, null);

        Assert.Throws<DomainException>(() => policy.AddBand("Outra", CropYear.Create("26/27"), 0, 10, null));
        Assert.Throws<DomainException>(() => policy.AddBand("Invertida", CropYear.Create("27/28"), 80, 30, null));
    }
}
