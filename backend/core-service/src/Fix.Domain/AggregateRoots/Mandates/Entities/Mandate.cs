using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Domain.AggregateRoots.Mandates;

/// <summary>
/// Mandato: autorização de volume dentro de um eixo da política. Nada se executa sem mandato;
/// o saldo é consumido pelas boletas aprovadas. Mandatos FORA da política ou emitidos sem alçada
/// ficam pendentes até a aprovação.
/// </summary>
public sealed class Mandate : AggregateRoot, IOrganizationScoped
{
    private Mandate()
    {
    }

    private Mandate(Guid organizationId, Guid policyId, Guid axisId, MandateType type, Guid issuedBy)
    {
        OrganizationId = organizationId;
        PolicyId = policyId;
        AxisId = axisId;
        Type = type;
        IssuedBy = issuedBy;
    }

    public Guid OrganizationId { get; private set; }

    public Guid PolicyId { get; private set; }

    public Guid AxisId { get; private set; }

    public MandateType Type { get; private set; }

    /// <summary>Direcionamento (ex.: "Fixar 30% da tela N26 do açúcar").</summary>
    public Title Title { get; private set; } = null!;

    /// <summary>Critério livre (ritmo, percentil, condição de mercado).</summary>
    public string? Criteria { get; private set; }

    public Commodity? Commodity { get; private set; }

    /// <summary>Tela/vencimento (ex.: N26, fev/27).</summary>
    public Tenor? Tenor { get; private set; }

    /// <summary>Volume autorizado; nulo = ordem de preço sem teto de volume.</summary>
    public decimal? Quantity { get; private set; }

    public MeasurementUnit? QuantityUnit { get; private set; }

    public PriceCriteria Price { get; private set; } = null!;

    /// <summary>Janela de execução.</summary>
    public DateOnly? WindowStart { get; private set; }

    public DateOnly? WindowEnd { get; private set; }

    public Compliance Compliance { get; private set; } = null!;

    public MandateStatus Status { get; private set; }

    public Guid IssuedBy { get; private set; }

    public Guid? DecidedBy { get; private set; }

    public DateTimeOffset? DecidedAt { get; private set; }

    public string? DecisionNote { get; private set; }

    public bool AcceptsOrders => Status == MandateStatus.Active;

    public static Mandate Issue(
        Policy policy,
        Guid axisId,
        MandateType type,
        MandateTerms terms,
        Compliance compliance,
        Guid issuedBy,
        bool issuerHasAuthority)
    {
        var axis = policy.GetAxis(axisId);
        if (axis.Factor != type.Factor())
        {
            throw new DomainException($"O eixo {axis.Code} cobre outro fator de risco; escolha um eixo compatível com o tipo do mandato.");
        }

        var mandate = new Mandate(policy.OrganizationId, policy.Id, axis.Id, type, issuedBy);
        mandate.Apply(terms, compliance);

        // Dentro da política e emitido por quem tem alçada: entra ativo. Senão, vai para a fila de aprovação.
        mandate.Status = compliance.IsWithin && issuerHasAuthority ? MandateStatus.Active : MandateStatus.PendingApproval;
        return mandate;
    }

    /// <summary>Somente mandatos pendentes podem ser editados (o enquadramento é recalculado).</summary>
    public void Update(MandateTerms terms, Compliance compliance)
    {
        if (Status != MandateStatus.PendingApproval)
        {
            throw new DomainException("Somente mandatos pendentes de aprovação podem ser editados. Encerre e emita um novo.");
        }

        Apply(terms, compliance);
    }

    public void Approve(Guid decidedBy, DateTimeOffset at, string? note)
    {
        EnsurePending();
        Decide(MandateStatus.Active, decidedBy, at, DomainGuard.OptionalText(note, 500, "observação"));
    }

    public void Reject(Guid decidedBy, DateTimeOffset at, string reason)
    {
        EnsurePending();
        var justification = DomainGuard.OptionalText(reason, 500, "justificativa")
            ?? throw new DomainException("A rejeição exige justificativa.");
        Decide(MandateStatus.Rejected, decidedBy, at, justification);
    }

    public void Close(Guid decidedBy, DateTimeOffset at, string? note)
    {
        if (Status != MandateStatus.Active)
        {
            throw new DomainException("Somente mandatos ativos podem ser encerrados.");
        }

        Decide(MandateStatus.Closed, decidedBy, at, DomainGuard.OptionalText(note, 500, "observação"));
    }

    private void Apply(MandateTerms terms, Compliance compliance)
    {
        if (Type is MandateType.Pricing && terms.Commodity is null)
        {
            throw new DomainException("Mandatos de precificação exigem a commodity.");
        }

        if (terms.Quantity is not null && (terms.Quantity <= 0 || terms.QuantityUnit is null))
        {
            throw new DomainException("A quantidade do mandato deve ser positiva e ter unidade.");
        }

        if (Type is MandateType.Currency && terms.QuantityUnit is not null and not MeasurementUnit.Usd)
        {
            throw new DomainException("Mandatos de moeda são expressos em US$ (nocional).");
        }

        if (terms.Quantity is null && !terms.Price.AtMarket && !terms.Price.HasAnyLevel)
        {
            throw new DomainException("Informe a quantidade ou um critério de preço (target, mínimo, máximo ou a mercado).");
        }

        if (terms.WindowStart is not null && terms.WindowEnd is not null && terms.WindowEnd < terms.WindowStart)
        {
            throw new DomainException("O fim da janela não pode ser anterior ao início.");
        }

        Title = terms.Title;
        Criteria = DomainGuard.Description(terms.Criteria);
        Commodity = terms.Commodity;
        Tenor = terms.Tenor;
        Quantity = terms.Quantity;
        QuantityUnit = terms.Quantity is null ? null : terms.QuantityUnit;
        Price = terms.Price;
        WindowStart = terms.WindowStart;
        WindowEnd = terms.WindowEnd;
        Compliance = compliance;
    }

    private void EnsurePending()
    {
        if (Status != MandateStatus.PendingApproval)
        {
            throw new DomainException("Este mandato não está pendente de aprovação.");
        }
    }

    private void Decide(MandateStatus status, Guid decidedBy, DateTimeOffset at, string? note)
    {
        Status = status;
        DecidedBy = decidedBy;
        DecidedAt = at;
        DecisionNote = note;
    }
}

/// <summary>Termos editáveis do mandato.</summary>
public sealed record MandateTerms(
    Title Title,
    string? Criteria,
    Commodity? Commodity,
    Tenor? Tenor,
    decimal? Quantity,
    MeasurementUnit? QuantityUnit,
    PriceCriteria Price,
    DateOnly? WindowStart,
    DateOnly? WindowEnd);

