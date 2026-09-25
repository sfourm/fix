using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Counterparties;
using Fix.Domain.AggregateRoots.Mandates;

namespace Fix.Domain.AggregateRoots.Orders;

/// <summary>
/// Boleta de hedge (order): executa e consome um mandato ativo. Futuros e opções consomem lotes;
/// NDF consome nocional em US$. Boletas emitidas sem alçada aguardam aprovação e não consomem saldo.
/// </summary>
public sealed class Order : AggregateRoot, IOrganizationScoped
{
    /// <summary>Prazo (dias úteis, aprox.) para o confirmation chegar antes de ficar em atraso.</summary>
    public const int ConfirmationDeadlineBusinessDays = 2;

    private Order()
    {
    }

    private Order(Guid organizationId, Guid mandateId, Guid requestedBy)
    {
        OrganizationId = organizationId;
        MandateId = mandateId;
        RequestedBy = requestedBy;
        Confirmation = ConfirmationStatus.Pending;
    }

    public Guid OrganizationId { get; private set; }

    public Guid MandateId { get; private set; }

    public OrderType Type { get; private set; }

    public TradeDirection Direction { get; private set; }

    public Guid CounterpartyId { get; private set; }

    public Commodity? Commodity { get; private set; }

    public Tenor Tenor { get; private set; } = null!;

    /// <summary>Lotes (futuros e opções).</summary>
    public decimal? Lots { get; private set; }

    /// <summary>Nocional em US$ (NDF).</summary>
    public decimal? NotionalUsd { get; private set; }

    /// <summary>Preço (futuro), taxa (NDF) ou strike (opção).</summary>
    public decimal Price { get; private set; }

    public string PriceUnit { get; private set; } = null!;

    public OptionKind? OptionKind { get; private set; }

    public decimal? Premium { get; private set; }

    public DateOnly TradeDate { get; private set; }

    public string? Notes { get; private set; }

    public ApprovalStatus Approval { get; private set; }

    public Guid RequestedBy { get; private set; }

    public Guid? DecidedBy { get; private set; }

    public DateTimeOffset? DecidedAt { get; private set; }

    public string? DecisionNote { get; private set; }

    public ConfirmationStatus Confirmation { get; private set; }

    public DateOnly? ConfirmedOn { get; private set; }

    public string? ConfirmationNote { get; private set; }

    /// <summary>Quantidade que consome o mandato (lotes ou US$). Pendentes e rejeitadas não consomem.</summary>
    public decimal ConsumedQuantity => Approval == ApprovalStatus.Approved ? Quantity : 0;

    public decimal Quantity => Type == OrderType.Ndf ? NotionalUsd ?? 0 : Lots ?? 0;

    public static Order Register(
        Mandate mandate,
        Counterparty counterparty,
        OrderTerms terms,
        decimal consumedByOthers,
        Guid requestedBy,
        bool requesterHasAuthority)
    {
        var order = new Order(mandate.OrganizationId, mandate.Id, requestedBy);
        order.Apply(mandate, counterparty, terms, consumedByOthers);
        order.Approval = requesterHasAuthority ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval;
        return order;
    }

    /// <summary>Permitido enquanto pendente de aprovação ou aprovada sem confirmation.</summary>
    public void Update(Mandate mandate, Counterparty counterparty, OrderTerms terms, decimal consumedByOthers)
    {
        if (Approval == ApprovalStatus.Rejected || Confirmation != ConfirmationStatus.Pending)
        {
            throw new DomainException("Boletas rejeitadas ou com confirmation registrado não podem ser editadas.");
        }

        Apply(mandate, counterparty, terms, consumedByOthers);
    }

    public void Approve(Guid decidedBy, DateTimeOffset at, string? note)
    {
        EnsurePendingApproval();
        Approval = ApprovalStatus.Approved;
        Decide(decidedBy, at, DomainGuard.OptionalText(note, 500, "observação"));
    }

    public void Reject(Guid decidedBy, DateTimeOffset at, string reason)
    {
        EnsurePendingApproval();
        Approval = ApprovalStatus.Rejected;
        Decide(decidedBy, at, DomainGuard.OptionalText(reason, 500, "justificativa")
            ?? throw new DomainException("A rejeição exige justificativa."));
    }

    public bool CanBeDeleted => Approval != ApprovalStatus.Approved;

    // ---------- Confirmation (middle office) ----------

    public void Confirm(DateOnly receivedOn)
    {
        EnsureApproved();
        if (receivedOn < TradeDate)
        {
            throw new DomainException("O confirmation não pode ser anterior à data do trade.");
        }

        Confirmation = ConfirmationStatus.Confirmed;
        ConfirmedOn = receivedOn;
        ConfirmationNote = null;
    }

    public void MarkDivergent(string description)
    {
        EnsureApproved();
        Confirmation = ConfirmationStatus.Divergent;
        ConfirmationNote = DomainGuard.OptionalText(description, 500, "divergência")
            ?? throw new DomainException("A divergência exige descrição.");
    }

    public void RefuseConfirmation(string reason)
    {
        EnsureApproved();
        Confirmation = ConfirmationStatus.Refused;
        ConfirmationNote = DomainGuard.OptionalText(reason, 500, "motivo")
            ?? throw new DomainException("A recusa exige motivo.");
    }

    /// <summary>Divergência sanada: confirmation aceito.</summary>
    public void ResolveDivergence(DateOnly on)
    {
        if (Confirmation is not (ConfirmationStatus.Divergent or ConfirmationStatus.Refused))
        {
            throw new DomainException("Não há divergência a sanar nesta boleta.");
        }

        Confirmation = ConfirmationStatus.Confirmed;
        ConfirmedOn ??= on;
        ConfirmationNote = null;
    }

    /// <summary>Pendente além do prazo (dias corridos convertidos de forma aproximada para dias úteis).</summary>
    public bool IsConfirmationOverdue(DateOnly today)
    {
        if (Approval != ApprovalStatus.Approved || Confirmation != ConfirmationStatus.Pending)
        {
            return false;
        }

        var businessDays = 0;
        for (var day = TradeDate.AddDays(1); day <= today; day = day.AddDays(1))
        {
            if (day.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                businessDays++;
            }
        }

        return businessDays > ConfirmationDeadlineBusinessDays;
    }

    // ---------- Regras ----------

    private void Apply(Mandate mandate, Counterparty counterparty, OrderTerms terms, decimal consumedByOthers)
    {
        if (mandate.Id != MandateId)
        {
            throw new DomainException("A boleta não pode trocar de mandato.");
        }

        if (!mandate.AcceptsOrders)
        {
            throw new DomainException("O mandato não está ativo — pendente, rejeitado ou encerrado não aceita boleta.");
        }

        if (!counterparty.IsHomologated)
        {
            throw new DomainException($"A contraparte {counterparty.Name} não está homologada (§5.5).");
        }

        var expected = mandate.Type switch
        {
            MandateType.Pricing => terms.Type is OrderType.Futures or OrderType.Option,
            MandateType.Currency => terms.Type is OrderType.Ndf,
            _ => false,
        };
        if (!expected)
        {
            throw new DomainException(mandate.Type is MandateType.Currency
                ? "Mandatos de moeda aceitam apenas boletas de NDF."
                : mandate.Type is MandateType.Pricing
                    ? "Mandatos de precificação aceitam apenas futuros e opções."
                    : "Mandatos comerciais e logísticos são executados por contratos, não por boletas de hedge.");
        }

        if (terms.Type == OrderType.Ndf)
        {
            if (terms.NotionalUsd is not > 0)
            {
                throw new DomainException("Informe o nocional em US$ do NDF.");
            }
        }
        else if (terms.Lots is not > 0)
        {
            throw new DomainException("Informe a quantidade de lotes.");
        }

        if (terms.Price <= 0)
        {
            throw new DomainException(terms.Type == OrderType.Option ? "Informe o strike da opção." : "Informe o preço.");
        }

        if (terms.Type == OrderType.Option && (terms.OptionKind is null || terms.Premium is not >= 0))
        {
            throw new DomainException("Opções exigem o tipo (put/call) e o prêmio.");
        }

        var quantity = terms.Type == OrderType.Ndf ? terms.NotionalUsd!.Value : terms.Lots!.Value;
        var consumes = terms.Type == OrderType.Ndf ? MeasurementUnit.Usd : MeasurementUnit.Lots;
        if (mandate.Quantity is { } authorized && mandate.QuantityUnit == consumes && consumedByOthers + quantity > authorized)
        {
            throw new DomainException(
                $"A boleta excede o saldo do mandato (autorizado {authorized:N0}, consumido {consumedByOthers:N0}, boleta {quantity:N0}).");
        }

        Type = terms.Type;
        Direction = terms.Direction;
        CounterpartyId = counterparty.Id;
        Commodity = terms.Type == OrderType.Ndf ? null : mandate.Commodity;
        Tenor = terms.Tenor;
        Lots = terms.Type == OrderType.Ndf ? null : terms.Lots;
        NotionalUsd = terms.Type == OrderType.Ndf ? terms.NotionalUsd : null;
        Price = terms.Price;
        PriceUnit = DomainGuard.OptionalText(terms.PriceUnit, 20, "unidade de preço")
            ?? (terms.Type == OrderType.Ndf ? "R$/US$" : "c/lb");
        OptionKind = terms.Type == OrderType.Option ? terms.OptionKind : null;
        Premium = terms.Type == OrderType.Option ? terms.Premium : null;
        TradeDate = terms.TradeDate;
        Notes = DomainGuard.Description(terms.Notes);
    }

    private void EnsurePendingApproval()
    {
        if (Approval != ApprovalStatus.PendingApproval)
        {
            throw new DomainException("Esta boleta não está pendente de aprovação.");
        }
    }

    private void EnsureApproved()
    {
        if (Approval != ApprovalStatus.Approved)
        {
            throw new DomainException("O confirmation só se aplica a boletas aprovadas.");
        }
    }

    private void Decide(Guid decidedBy, DateTimeOffset at, string? note)
    {
        DecidedBy = decidedBy;
        DecidedAt = at;
        DecisionNote = note;
    }
}

/// <summary>Termos editáveis da boleta.</summary>
public sealed record OrderTerms(
    OrderType Type,
    TradeDirection Direction,
    Tenor Tenor,
    decimal? Lots,
    decimal? NotionalUsd,
    decimal Price,
    string? PriceUnit,
    OptionKind? OptionKind,
    decimal? Premium,
    DateOnly TradeDate,
    string? Notes);

