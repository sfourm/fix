using System.Globalization;
using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Counterparties;
using Fix.Domain.AggregateRoots.Mandates;

namespace Fix.Domain.AggregateRoots.Orders;

/// <summary>
/// Boleta de hedge (order): a operação executada pela mesa. Futuros e opções consomem lotes; NDF consome nocional em US$.
/// Pendentes e rejeitadas não consomem saldo (FIX2 · I-02).
///
/// "Desvio não bloqueia — expõe" (FIX2 · I-01): boleta sem mandato, acima do saldo, com tela diferente da do mandato ou
/// venda descoberta de opção (vedada, I-08) é registrada com justificativa obrigatória, fica FORA, vai para aprovação e
/// mantém o carimbo. Mandato vinculado depois da execução fica marcado "a posteriori" para sempre.
/// </summary>
public sealed class Order : AggregateRoot, IOrganizationScoped
{
    /// <summary>Prazo (dias úteis, aprox.) para o confirmation chegar antes de ficar em atraso.</summary>
    public const int ConfirmationDeadlineBusinessDays = 2;

    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private Order()
    {
    }

    private Order(Guid organizationId, int number, Guid requestedBy)
    {
        OrganizationId = organizationId;
        Number = number;
        RequestedBy = requestedBy;
        Confirmation = ConfirmationStatus.Pending;
    }

    public Guid OrganizationId { get; private set; }

    /// <summary>Sequencial na organização; o código legível é HX-0001 (I-10).</summary>
    public int Number { get; private set; }

    public string Code => EntityCodes.Order(Number);

    /// <summary>Mandato que autoriza a boleta; nulo = boleta sem mandato (desvio sinalizado).</summary>
    public Guid? MandateId { get; private set; }

    /// <summary>Mandato vinculado depois da execução: carimbo permanente.</summary>
    public bool LinkedAfterExecution { get; private set; }

    /// <summary>Excedeu o saldo do mandato quando foi registrada, editada ou vinculada.</summary>
    public bool ExceedsMandate { get; private set; }

    /// <summary>Justificativa do desvio (sem mandato, estouro, tela diferente, venda descoberta, vínculo a posteriori).</summary>
    public string? DeviationNote { get; private set; }

    /// <summary>Venda de opção coberta (lastreada em produção/posição); a descoberta é vedada.</summary>
    public bool CoveredSale { get; private set; }

    /// <summary>Enquadramento calculado da boleta (nunca digitado).</summary>
    public Compliance Compliance { get; private set; } = null!;

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

    /// <summary>Quem registrou o último passo do confirmation (middle office).</summary>
    public Guid? ConfirmationBy { get; private set; }

    /// <summary>Quantidade que consome o mandato (lotes ou US$). Pendentes, rejeitadas e sem mandato não consomem.</summary>
    public decimal ConsumedQuantity => Approval == ApprovalStatus.Approved && MandateId is not null ? Quantity : 0;

    public decimal Quantity => Type == OrderType.Ndf ? NotionalUsd ?? 0 : Lots ?? 0;

    public bool CanBeDeleted => Approval != ApprovalStatus.Approved;

    /// <summary>
    /// Registra a boleta. Dentro do enquadramento e com alçada (self_approve) já nasce aprovada; FORA ou sem alçada vai
    /// para a fila de aprovação.
    /// </summary>
    public static Order Register(
        int number,
        Mandate? mandate,
        Counterparty counterparty,
        OrderTerms terms,
        decimal consumedByOthers,
        Guid requestedBy,
        bool requesterHasAuthority)
    {
        var order = new Order(counterparty.OrganizationId, number, requestedBy) { MandateId = mandate?.Id };
        order.Apply(mandate, counterparty, terms, consumedByOthers);
        order.Approval = order.Compliance.IsWithin && requesterHasAuthority ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval;
        return order;
    }

    /// <summary>Permitido enquanto pendente de aprovação ou aprovada sem confirmation; aprovada não pode passar a FORA.</summary>
    public void Update(Mandate? mandate, Counterparty counterparty, OrderTerms terms, decimal consumedByOthers)
    {
        if (Approval == ApprovalStatus.Rejected || Confirmation != ConfirmationStatus.Pending)
        {
            throw new DomainException("Boletas rejeitadas ou com confirmação registrada não podem ser editadas.");
        }

        var wasApproved = Approval == ApprovalStatus.Approved;
        Apply(mandate, counterparty, terms, consumedByOthers);
        if (wasApproved && !Compliance.IsWithin)
        {
            throw new DomainException($"A alteração deixaria a boleta aprovada FORA ({Compliance.Reason}). Registre uma nova boleta com justificativa.");
        }
    }

    /// <summary>
    /// Vínculo a posteriori: liga a boleta sem mandato a um mandato ativo depois da execução. Exige justificativa, recalcula
    /// o enquadramento e deixa o carimbo "a posteriori" para sempre.
    /// </summary>
    public void LinkMandate(Mandate mandate, decimal consumedByOthers, string justification)
    {
        if (MandateId is not null)
        {
            throw new DomainException("A boleta já está vinculada a um mandato.");
        }

        if (Approval == ApprovalStatus.Rejected)
        {
            throw new DomainException("Boleta rejeitada não é vinculada a mandato.");
        }

        var note = DomainGuard.OptionalText(justification, 500, "justificativa")
            ?? throw new DomainException("O vínculo a posteriori exige justificativa.");

        MandateId = mandate.Id;
        LinkedAfterExecution = true;
        Evaluate(mandate, consumedByOthers, note);
    }

    /// <summary>
    /// Na aprovação o enquadramento é refeito com o consumo atual (outras boletas podem ter consumido o mandato). Estourar o
    /// saldo sem justificativa registrada não é aprovado: a boleta precisa ser editada e justificada.
    /// </summary>
    public void Approve(Mandate? mandate, decimal consumedByOthers, Guid decidedBy, DateTimeOffset at, string? note)
    {
        EnsurePendingApproval();
        Evaluate(mandate, consumedByOthers, DeviationNote);
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

    // ---------- Confirmation (middle office) ----------

    public void Confirm(DateOnly receivedOn, Guid by)
    {
        EnsureCanConfirm(by);
        if (receivedOn < TradeDate)
        {
            throw new DomainException("A confirmação não pode ser anterior à data da operação.");
        }

        Confirmation = ConfirmationStatus.Confirmed;
        ConfirmedOn = receivedOn;
        ConfirmationNote = null;
        ConfirmationBy = by;
    }

    public void MarkDivergent(string description, Guid by)
    {
        EnsureCanConfirm(by);
        ConfirmationNote = DomainGuard.OptionalText(description, 500, "divergência")
            ?? throw new DomainException("A divergência exige descrição.");
        Confirmation = ConfirmationStatus.Divergent;
        ConfirmationBy = by;
    }

    public void RefuseConfirmation(string reason, Guid by)
    {
        EnsureCanConfirm(by);
        ConfirmationNote = DomainGuard.OptionalText(reason, 500, "motivo")
            ?? throw new DomainException("A recusa exige motivo.");
        Confirmation = ConfirmationStatus.Refused;
        ConfirmationBy = by;
    }

    /// <summary>Divergência ou recusa sanada: confirmation aceito.</summary>
    public void ResolveDivergence(DateOnly on, Guid by)
    {
        if (Confirmation is not (ConfirmationStatus.Divergent or ConfirmationStatus.Refused))
        {
            throw new DomainException("Não há divergência a sanar nesta boleta.");
        }

        EnsureCanConfirm(by);
        Confirmation = ConfirmationStatus.Confirmed;
        ConfirmedOn ??= on;
        ConfirmationNote = null;
        ConfirmationBy = by;
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

    private void Apply(Mandate? mandate, Counterparty counterparty, OrderTerms terms, decimal consumedByOthers)
    {
        if (mandate?.Id != MandateId)
        {
            throw new DomainException("A boleta não pode trocar de mandato (sem mandato, use o vínculo a posteriori).");
        }

        if (!counterparty.IsHomologated)
        {
            throw new DomainException($"A contraparte {counterparty.Name} não está homologada (§5.5).");
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

        var commodity = terms.Type == OrderType.Ndf ? null : mandate?.Commodity ?? terms.Commodity;
        if (terms.Type != OrderType.Ndf && commodity is null)
        {
            throw new DomainException("Informe a commodity da boleta.");
        }

        Type = terms.Type;
        Direction = terms.Direction;
        CounterpartyId = counterparty.Id;
        Commodity = commodity;
        Tenor = terms.Tenor;
        Lots = terms.Type == OrderType.Ndf ? null : terms.Lots;
        NotionalUsd = terms.Type == OrderType.Ndf ? terms.NotionalUsd : null;
        Price = terms.Price;
        PriceUnit = DomainGuard.OptionalText(terms.PriceUnit, 20, "unidade de preço")
            ?? (terms.Type == OrderType.Ndf ? "R$/US$" : "c/lb");
        OptionKind = terms.Type == OrderType.Option ? terms.OptionKind : null;
        Premium = terms.Type == OrderType.Option ? terms.Premium : null;
        CoveredSale = terms.Type == OrderType.Option && terms.Direction == TradeDirection.Sell && terms.CoveredSale;
        TradeDate = terms.TradeDate;
        Notes = DomainGuard.Description(terms.Notes);

        Evaluate(mandate, consumedByOthers, DomainGuard.OptionalText(terms.Justification, 500, "justificativa"));
    }

    /// <summary>
    /// Enquadramento da boleta (bolEnq resumido): mandato ativo e compatível, saldo, tela do mandato e venda de opção.
    /// Incompatibilidade de produto (tipo/commodity) bloqueia; os demais desvios expõem e exigem justificativa.
    /// </summary>
    private void Evaluate(Mandate? mandate, decimal consumedByOthers, string? justification)
    {
        var deviations = new List<string>();

        if (mandate is null)
        {
            deviations.Add("sem mandato");
        }
        else
        {
            if (!mandate.AcceptsOrders)
            {
                throw new DomainException(
                    $"O mandato {mandate.Code} não está ativo — pendente, rejeitado ou encerrado não autoriza boleta. Registre sem mandato, com justificativa, se a operação já foi feita.");
            }

            var compatible = mandate.Type switch
            {
                MandateType.Pricing => Type is OrderType.Futures or OrderType.Option,
                MandateType.Currency => Type is OrderType.Ndf,
                _ => false,
            };
            if (!compatible)
            {
                throw new DomainException(mandate.Type is MandateType.Currency
                    ? "Mandatos de moeda aceitam apenas boletas de NDF."
                    : mandate.Type is MandateType.Pricing
                        ? "Mandatos de precificação aceitam apenas futuros e opções."
                        : "Mandatos comerciais e logísticos são executados por contratos, não por boletas de hedge.");
            }

            var consumes = Type == OrderType.Ndf ? MeasurementUnit.Usd : MeasurementUnit.Lots;
            ExceedsMandate = mandate.Quantity is { } authorized && mandate.QuantityUnit == consumes && consumedByOthers + Quantity > authorized;
            if (ExceedsMandate)
            {
                deviations.Add(
                    $"excede o saldo do {mandate.Code} (autorizado {mandate.Quantity!.Value.ToString("N0", PtBr)}, consumido {consumedByOthers.ToString("N0", PtBr)}, boleta {Quantity.ToString("N0", PtBr)})");
            }

            if (mandate.Tenor is not null && !mandate.Tenor.Equals(Tenor))
            {
                deviations.Add($"tela {Tenor} diferente da do {mandate.Code} ({mandate.Tenor})");
            }
        }

        if (Type == OrderType.Option && Direction == TradeDirection.Sell && !CoveredSale)
        {
            deviations.Add("venda descoberta de opção — instrumento vedado (§8)");
        }

        if (deviations.Count > 0 && justification is null)
        {
            throw new DomainException(
                $"Desvio: {string.Join(" · ", deviations)}. A boleta pode ser registrada, mas exige justificativa — fica FORA, vai para aprovação e o desvio fica exposto.");
        }

        DeviationNote = deviations.Count > 0 || LinkedAfterExecution ? justification : null;
        Compliance = deviations.Count > 0
            ? Compliance.Outside(string.Join(" · ", deviations))
            : Compliance.Within(
                $"dentro do {mandate!.Code}" + (CoveredSale ? " · venda coberta (teto de 15% do disponível a conferir)" : string.Empty));
    }

    private void EnsurePendingApproval()
    {
        if (Approval != ApprovalStatus.PendingApproval)
        {
            throw new DomainException("Esta boleta não está pendente de aprovação.");
        }
    }

    /// <summary>Segregação (FIX2 · I-06): o middle office confere; quem executou a boleta não registra o confirmation dela.</summary>
    private void EnsureCanConfirm(Guid by)
    {
        if (Approval != ApprovalStatus.Approved)
        {
            throw new DomainException("A confirmação só se aplica a boletas aprovadas.");
        }

        if (by == RequestedBy)
        {
            throw new DomainException("Quem executou a boleta não registra a confirmação dela: a conferência é do middle office (segregação).");
        }
    }

    private void Decide(Guid decidedBy, DateTimeOffset at, string? note)
    {
        DecidedBy = decidedBy;
        DecidedAt = at;
        DecisionNote = note;
    }
}

/// <summary>Termos editáveis da boleta. Commodity só é lida sem mandato (com mandato, vale a dele).</summary>
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
    string? Notes,
    Commodity? Commodity = null,
    bool CoveredSale = false,
    string? Justification = null);
