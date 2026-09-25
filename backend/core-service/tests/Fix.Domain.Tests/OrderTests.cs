using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Orders;

namespace Fix.Domain.Tests;

public sealed class OrderTests
{
    private static OrderTerms Futures(decimal lots = 150) => new(
        OrderType.Futures,
        TradeDirection.Sell,
        Tenor.Create("N26"),
        lots,
        null,
        16.42m,
        "c/lb",
        null,
        null,
        new DateOnly(2026, 8, 10),
        null);

    [Fact]
    public void Register_with_authority_is_approved_and_consumes_the_mandate()
    {
        var mandate = TestData.ActivePricingMandate();

        var order = Order.Register(mandate, TestData.CreateCounterparty(), Futures(), 0, TestData.UserId, requesterHasAuthority: true);

        Assert.Equal(ApprovalStatus.Approved, order.Approval);
        Assert.Equal(150, order.ConsumedQuantity);
        Assert.Equal(Commodity.RawSugar, order.Commodity);
    }

    [Fact]
    public void Pending_order_does_not_consume_the_mandate()
    {
        var order = Order.Register(
            TestData.ActivePricingMandate(), TestData.CreateCounterparty(), Futures(), 0, TestData.UserId, requesterHasAuthority: false);

        Assert.Equal(ApprovalStatus.PendingApproval, order.Approval);
        Assert.Equal(0, order.ConsumedQuantity);
    }

    [Fact]
    public void Order_cannot_exceed_mandate_balance()
    {
        var mandate = TestData.ActivePricingMandate(lots: 200);

        Assert.Throws<DomainException>(() =>
            Order.Register(mandate, TestData.CreateCounterparty(), Futures(lots: 100), consumedByOthers: 150, TestData.UserId, true));
    }

    [Fact]
    public void Counterparty_must_be_homologated_and_mandate_active()
    {
        var mandate = TestData.ActivePricingMandate();

        Assert.Throws<DomainException>(() =>
            Order.Register(mandate, TestData.CreateCounterparty(homologated: false), Futures(), 0, TestData.UserId, true));

        mandate.Close(TestData.UserId, DateTimeOffset.UtcNow, null);
        Assert.Throws<DomainException>(() =>
            Order.Register(mandate, TestData.CreateCounterparty(), Futures(), 0, TestData.UserId, true));
    }

    [Fact]
    public void Pricing_mandate_does_not_accept_ndf()
    {
        var ndf = Futures() with { Type = OrderType.Ndf, Lots = null, NotionalUsd = 1_000_000, Price = 5.52m };

        Assert.Throws<DomainException>(() =>
            Order.Register(TestData.ActivePricingMandate(), TestData.CreateCounterparty(), ndf, 0, TestData.UserId, true));
    }

    [Fact]
    public void Confirmation_flow_divergence_and_resolution()
    {
        var order = Order.Register(TestData.ActivePricingMandate(), TestData.CreateCounterparty(), Futures(), 0, TestData.UserId, true);

        Assert.True(order.IsConfirmationOverdue(TestData.Today));
        Assert.Throws<DomainException>(() => order.MarkDivergent(" "));

        order.MarkDivergent("notional difere da boleta");
        Assert.Equal(ConfirmationStatus.Divergent, order.Confirmation);

        order.ResolveDivergence(TestData.Today);
        Assert.Equal(ConfirmationStatus.Confirmed, order.Confirmation);
        Assert.False(order.IsConfirmationOverdue(TestData.Today));
    }

    [Fact]
    public void Confirmation_only_applies_to_approved_orders()
    {
        var order = Order.Register(TestData.ActivePricingMandate(), TestData.CreateCounterparty(), Futures(), 0, TestData.UserId, false);

        Assert.Throws<DomainException>(() => order.Confirm(TestData.Today));
    }
}
