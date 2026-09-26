using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Orders;

namespace Fix.Domain.Tests;

public sealed class OrderTests
{
    private static OrderTerms Futures(decimal lots = 150, string tenor = "N26", string? justification = null) => new(
        OrderType.Futures,
        TradeDirection.Sell,
        Tenor.Create(tenor),
        lots,
        null,
        16.42m,
        "c/lb",
        null,
        null,
        new DateOnly(2026, 8, 10),
        null,
        Justification: justification);

    [Fact]
    public void Register_with_authority_is_approved_and_consumes_the_mandate()
    {
        var mandate = TestData.ActivePricingMandate();

        var order = Order.Register(7, mandate, TestData.CreateCounterparty(), Futures(), 0, TestData.UserId, requesterHasAuthority: true);

        Assert.Equal(ApprovalStatus.Approved, order.Approval);
        Assert.True(order.Compliance.IsWithin);
        Assert.Equal(150, order.ConsumedQuantity);
        Assert.Equal(Commodity.RawSugar, order.Commodity);
        Assert.Equal("HX-0007", order.Code);
        Assert.Equal("MD-01", mandate.Code);
    }

    [Fact]
    public void Pending_order_does_not_consume_the_mandate()
    {
        var order = Order.Register(
            1, TestData.ActivePricingMandate(), TestData.CreateCounterparty(), Futures(), 0, TestData.UserId, requesterHasAuthority: false);

        Assert.Equal(ApprovalStatus.PendingApproval, order.Approval);
        Assert.Equal(0, order.ConsumedQuantity);
    }

    [Fact]
    public void Exceeding_the_balance_requires_justification_and_is_exposed_as_outside()
    {
        var mandate = TestData.ActivePricingMandate(lots: 200);

        // Sem justificativa não registra; com justificativa registra FORA, na fila, com o carimbo do estouro.
        Assert.Throws<DomainException>(() =>
            Order.Register(1, mandate, TestData.CreateCounterparty(), Futures(lots: 100), consumedByOthers: 150, TestData.UserId, true));

        var order = Order.Register(
            1, mandate, TestData.CreateCounterparty(), Futures(lots: 100, justification: "janela de preço fechando"), 150, TestData.UserId, true);

        Assert.True(order.ExceedsMandate);
        Assert.False(order.Compliance.IsWithin);
        Assert.Contains("excede o saldo", order.Compliance.Reason);
        Assert.Equal(ApprovalStatus.PendingApproval, order.Approval);
        Assert.Equal("janela de preço fechando", order.DeviationNote);
    }

    [Fact]
    public void Order_without_mandate_is_possible_but_always_flagged()
    {
        Assert.Throws<DomainException>(() =>
            Order.Register(1, null, TestData.CreateCounterparty(), Futures() with { Commodity = Commodity.RawSugar }, 0, TestData.UserId, true));

        var order = Order.Register(
            1, null, TestData.CreateCounterparty(), Futures(justification: "mesa operou antes da emissão") with { Commodity = Commodity.RawSugar }, 0, TestData.UserId, true);

        Assert.Null(order.MandateId);
        Assert.Contains("sem mandato", order.Compliance.Reason);
        Assert.Equal(ApprovalStatus.PendingApproval, order.Approval);
        Assert.Equal(0, order.ConsumedQuantity);
    }

    [Fact]
    public void Linking_a_mandate_afterwards_stamps_the_order_forever()
    {
        var order = Order.Register(
            1, null, TestData.CreateCounterparty(), Futures(justification: "operou antes") with { Commodity = Commodity.RawSugar }, 0, TestData.UserId, true);
        var mandate = TestData.ActivePricingMandate();

        Assert.Throws<DomainException>(() => order.LinkMandate(mandate, 0, " "));
        order.LinkMandate(mandate, 0, "mandato MD-01 cobria a operação");

        Assert.Equal(mandate.Id, order.MandateId);
        Assert.True(order.LinkedAfterExecution);
        Assert.True(order.Compliance.IsWithin);
        Assert.Throws<DomainException>(() => order.LinkMandate(mandate, 0, "de novo"));
    }

    [Fact]
    public void Different_tenor_from_the_mandate_is_a_deviation()
    {
        var mandate = TestData.ActivePricingMandate();

        Assert.Throws<DomainException>(() => Order.Register(1, mandate, TestData.CreateCounterparty(), Futures(tenor: "V26"), 0, TestData.UserId, true));

        var order = Order.Register(1, mandate, TestData.CreateCounterparty(), Futures(tenor: "V26", justification: "rolagem"), 0, TestData.UserId, true);
        Assert.Contains("tela V26", order.Compliance.Reason);
    }

    [Fact]
    public void Uncovered_option_sale_is_outside_by_definition()
    {
        var sale = Futures(justification: "exceção do Comitê") with { Type = OrderType.Option, OptionKind = OptionKind.Call, Premium = 0.4m, Price = 18m };

        var uncovered = Order.Register(1, TestData.ActivePricingMandate(), TestData.CreateCounterparty(), sale, 0, TestData.UserId, true);
        Assert.Contains("venda descoberta", uncovered.Compliance.Reason);
        Assert.Equal(ApprovalStatus.PendingApproval, uncovered.Approval);

        var covered = Order.Register(
            2, TestData.ActivePricingMandate(), TestData.CreateCounterparty(), sale with { CoveredSale = true, Justification = null }, 0, TestData.UserId, true);
        Assert.True(covered.Compliance.IsWithin);
        Assert.Equal(ApprovalStatus.Approved, covered.Approval);
    }

    [Fact]
    public void Counterparty_must_be_homologated_and_mandate_active()
    {
        var mandate = TestData.ActivePricingMandate();

        Assert.Throws<DomainException>(() =>
            Order.Register(1, mandate, TestData.CreateCounterparty(homologated: false), Futures(), 0, TestData.UserId, true));

        mandate.Close(TestData.UserId, DateTimeOffset.UtcNow, null);
        Assert.Throws<DomainException>(() =>
            Order.Register(1, mandate, TestData.CreateCounterparty(), Futures(), 0, TestData.UserId, true));
    }

    [Fact]
    public void Pricing_mandate_does_not_accept_ndf()
    {
        var ndf = Futures() with { Type = OrderType.Ndf, Lots = null, NotionalUsd = 1_000_000, Price = 5.52m };

        Assert.Throws<DomainException>(() =>
            Order.Register(1, TestData.ActivePricingMandate(), TestData.CreateCounterparty(), ndf, 0, TestData.UserId, true));
    }

    [Fact]
    public void Approval_rechecks_the_balance_consumed_meanwhile()
    {
        var mandate = TestData.ActivePricingMandate(lots: 200);
        var order = Order.Register(1, mandate, TestData.CreateCounterparty(), Futures(lots: 100), 0, TestData.UserId, false);

        // Outras boletas consumiram 150 enquanto esta aguardava: aprovar estouraria sem justificativa.
        Assert.Throws<DomainException>(() => order.Approve(mandate, 150, TestData.MiddleOfficeId, DateTimeOffset.UtcNow, null));

        order.Approve(mandate, 50, TestData.MiddleOfficeId, DateTimeOffset.UtcNow, null);
        Assert.Equal(ApprovalStatus.Approved, order.Approval);
    }

    [Fact]
    public void Confirmation_flow_divergence_and_resolution()
    {
        var order = Order.Register(1, TestData.ActivePricingMandate(), TestData.CreateCounterparty(), Futures(), 0, TestData.UserId, true);

        Assert.True(order.IsConfirmationOverdue(TestData.Today));
        Assert.Throws<DomainException>(() => order.MarkDivergent(" ", TestData.MiddleOfficeId));

        order.MarkDivergent("notional difere da boleta", TestData.MiddleOfficeId);
        Assert.Equal(ConfirmationStatus.Divergent, order.Confirmation);

        order.ResolveDivergence(TestData.Today, TestData.MiddleOfficeId);
        Assert.Equal(ConfirmationStatus.Confirmed, order.Confirmation);
        Assert.False(order.IsConfirmationOverdue(TestData.Today));
    }

    [Fact]
    public void Who_executed_the_order_does_not_register_its_confirmation()
    {
        var order = Order.Register(1, TestData.ActivePricingMandate(), TestData.CreateCounterparty(), Futures(), 0, TestData.UserId, true);

        Assert.Throws<DomainException>(() => order.Confirm(TestData.Today, TestData.UserId));
        order.Confirm(TestData.Today, TestData.MiddleOfficeId);
        Assert.Equal(TestData.MiddleOfficeId, order.ConfirmationBy);
    }

    [Fact]
    public void Confirmation_only_applies_to_approved_orders()
    {
        var order = Order.Register(1, TestData.ActivePricingMandate(), TestData.CreateCounterparty(), Futures(), 0, TestData.UserId, false);

        Assert.Throws<DomainException>(() => order.Confirm(TestData.Today, TestData.MiddleOfficeId));
    }
}
