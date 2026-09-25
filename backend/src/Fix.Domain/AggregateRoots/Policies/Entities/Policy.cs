using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Policies;

/// <summary>
/// Política de riscos (política-mãe) versionada. É redigida fora do app e incorporada aqui como parâmetros:
/// limites, bandas de cobertura, instrumentos e eixos. Só uma política vigente autoriza mandatos dentro da política.
/// </summary>
public sealed class Policy : AggregateRoot, IOrganizationScoped
{
    private readonly List<PolicyAxis> _axes = [];
    private readonly List<CoverageBand> _bands = [];
    private readonly List<PolicyInstrument> _instruments = [];
    private readonly List<PolicyVersion> _versions = [];

    private Policy()
    {
    }

    private Policy(Guid organizationId, string code, Title title, string version, string? description, DateRange validity)
    {
        OrganizationId = organizationId;
        Code = code;
        Title = title;
        Version = version;
        Description = DomainGuard.Description(description);
        Validity = validity;
        Status = PolicyStatus.Draft;
        Limits = PolicyLimits.Default();
    }

    public Guid OrganizationId { get; private set; }

    /// <summary>Identificador da política (ex.: POL-2026).</summary>
    public string Code { get; private set; } = null!;

    public Title Title { get; private set; } = null!;

    /// <summary>Versão corrente (ex.: v1.0).</summary>
    public string Version { get; private set; } = null!;

    public string? Description { get; private set; }

    public PolicyStatus Status { get; private set; }

    /// <summary>Vigência da política.</summary>
    public DateRange Validity { get; private set; } = null!;

    /// <summary>Número/data da ata do Conselho que aprovou a versão vigente.</summary>
    public string? ApprovalRecord { get; private set; }

    public DateOnly? ApprovedOn { get; private set; }

    public PolicyLimits Limits { get; private set; } = null!;

    public IReadOnlyCollection<PolicyAxis> Axes => _axes.AsReadOnly();

    public IReadOnlyCollection<CoverageBand> Bands => _bands.AsReadOnly();

    public IReadOnlyCollection<PolicyInstrument> Instruments => _instruments.AsReadOnly();

    public IReadOnlyCollection<PolicyVersion> Versions => _versions.AsReadOnly();

    public bool IsEditable => Status is PolicyStatus.Draft or PolicyStatus.UnderApproval;

    public static Policy Create(
        Guid organizationId,
        string code,
        Title title,
        string version,
        string? description,
        DateRange validity,
        DateOnly today)
    {
        if (organizationId == Guid.Empty)
        {
            throw new DomainException("A política precisa pertencer a uma organização.");
        }

        var policy = new Policy(organizationId, NormalizeCode(code), title, NormalizeVersion(version), description, validity);
        policy._versions.Add(new PolicyVersion(policy.Id, policy.Version, PolicyStatus.Draft, today, "rascunho criado"));
        return policy;
    }

    /// <summary>Vigente e dentro do período de vigência na data informada.</summary>
    public bool IsInForceOn(DateOnly date) => Status == PolicyStatus.Active && Validity.Contains(date);

    // ---------- Edição (somente rascunho / em aprovação) ----------

    public void UpdateGeneral(string code, Title title, string? description, DateRange validity)
    {
        EnsureEditable();
        Code = NormalizeCode(code);
        Title = title;
        Description = DomainGuard.Description(description);
        Validity = validity;
    }

    public void UpdateLimits(PolicyLimits limits)
    {
        EnsureEditable();
        Limits = limits;
    }

    public PolicyAxis AddAxis(
        string code,
        Title title,
        RiskFactor factor,
        string? statement,
        string? limitDescription,
        string? approver,
        IEnumerable<string> restrictions)
    {
        EnsureEditable();
        var axis = new PolicyAxis(Id);
        axis.Update(code, title, factor, statement, limitDescription, approver, restrictions);
        EnsureUniqueAxisCode(axis.Code, axis.Id, _axes);
        _axes.Add(axis);
        return axis;
    }

    public void UpdateAxis(
        Guid axisId,
        string code,
        Title title,
        RiskFactor factor,
        string? statement,
        string? limitDescription,
        string? approver,
        IEnumerable<string> restrictions)
    {
        EnsureEditable();
        var axis = GetAxis(axisId);
        axis.Update(code, title, factor, statement, limitDescription, approver, restrictions);
        EnsureUniqueAxisCode(axis.Code, axis.Id, _axes);
    }

    public void RemoveAxis(Guid axisId)
    {
        EnsureEditable();
        _axes.Remove(GetAxis(axisId));
    }

    public PolicyAxis GetAxis(Guid axisId) =>
        _axes.FirstOrDefault(a => a.Id == axisId) ?? throw new DomainException("Eixo não encontrado na política.");

    public CoverageBand AddBand(string horizon, CropYear crop, decimal minPct, decimal maxPct, string? note)
    {
        EnsureEditable();
        if (_bands.Any(b => b.Crop == crop))
        {
            throw new DomainException($"Já existe banda para a safra {crop}.");
        }

        var band = new CoverageBand(Id);
        band.Update(horizon, crop, minPct, maxPct, note);
        _bands.Add(band);
        return band;
    }

    public void UpdateBand(Guid bandId, string horizon, CropYear crop, decimal minPct, decimal maxPct, string? note)
    {
        EnsureEditable();
        if (_bands.Any(b => b.Crop == crop && b.Id != bandId))
        {
            throw new DomainException($"Já existe banda para a safra {crop}.");
        }

        GetBand(bandId).Update(horizon, crop, minPct, maxPct, note);
    }

    public void RemoveBand(Guid bandId)
    {
        EnsureEditable();
        _bands.Remove(GetBand(bandId));
    }

    public PolicyInstrument AddInstrument(string name, InstrumentPermission permission, string? condition)
    {
        EnsureEditable();
        var instrument = new PolicyInstrument(Id);
        instrument.Update(name, permission, condition);
        _instruments.Add(instrument);
        return instrument;
    }

    public void UpdateInstrument(Guid instrumentId, string name, InstrumentPermission permission, string? condition)
    {
        EnsureEditable();
        GetInstrument(instrumentId).Update(name, permission, condition);
    }

    public void RemoveInstrument(Guid instrumentId)
    {
        EnsureEditable();
        _instruments.Remove(GetInstrument(instrumentId));
    }

    // ---------- Ciclo de vida ----------

    /// <summary>Rascunho → em aprovação (submetida ao Conselho).</summary>
    public void Submit(DateOnly today)
    {
        if (Status != PolicyStatus.Draft)
        {
            throw new DomainException("Somente políticas em rascunho podem ser submetidas.");
        }

        if (_axes.Count == 0)
        {
            throw new DomainException("A política precisa de ao menos um eixo antes de ser submetida.");
        }

        Status = PolicyStatus.UnderApproval;
        _versions.Add(new PolicyVersion(Id, Version, Status, today, "submetida ao Conselho"));
    }

    /// <summary>Em aprovação → vigente. Sem ata não entra em vigor (§15).</summary>
    public void Approve(string approvalRecord, DateOnly today)
    {
        if (Status != PolicyStatus.UnderApproval)
        {
            throw new DomainException("Somente políticas em aprovação podem ser aprovadas.");
        }

        ApprovalRecord = DomainGuard.OptionalText(approvalRecord, 100, "ata")
            ?? throw new DomainException("Sem ata do Conselho a política não entra em vigor (§15).");
        ApprovedOn = today;
        Status = PolicyStatus.Active;
        _versions.Add(new PolicyVersion(Id, Version, Status, today, $"vigente · ata {ApprovalRecord}"));
    }

    /// <summary>Vigente → nova versão em aprovação (a versão anterior deixa de valer até a nova ata).</summary>
    public void OpenNewVersion(string version, string? reason, DateOnly today)
    {
        if (Status != PolicyStatus.Active)
        {
            throw new DomainException("Só é possível abrir nova versão de uma política vigente.");
        }

        var normalized = NormalizeVersion(version);
        if (_versions.Any(v => v.Version == normalized))
        {
            throw new DomainException($"A versão {normalized} já existe no histórico.");
        }

        Version = normalized;
        Status = PolicyStatus.UnderApproval;
        ApprovalRecord = null;
        ApprovedOn = null;
        _versions.Add(new PolicyVersion(Id, Version, Status, today, DomainGuard.OptionalText(reason, 300, "motivo") ?? "revisão"));
    }

    /// <summary>Vigente → substituída (quando outra política da companhia entra em vigor).</summary>
    public void Supersede(DateOnly today)
    {
        if (Status != PolicyStatus.Active)
        {
            return;
        }

        Status = PolicyStatus.Superseded;
        _versions.Add(new PolicyVersion(Id, Version, Status, today, "substituída por nova política vigente"));
    }

    // ---------- Helpers ----------

    private void EnsureEditable()
    {
        if (!IsEditable)
        {
            throw new DomainException("A política vigente/substituída não pode ser alterada. Abra uma nova versão.");
        }
    }

    private CoverageBand GetBand(Guid bandId) =>
        _bands.FirstOrDefault(b => b.Id == bandId) ?? throw new DomainException("Banda não encontrada na política.");

    private PolicyInstrument GetInstrument(Guid instrumentId) =>
        _instruments.FirstOrDefault(i => i.Id == instrumentId) ?? throw new DomainException("Instrumento não encontrado na política.");

    private static void EnsureUniqueAxisCode(string code, Guid axisId, IEnumerable<PolicyAxis> axes)
    {
        if (axes.Any(a => a.Code == code && a.Id != axisId))
        {
            throw new DomainException($"Já existe um eixo com o código {code}.");
        }
    }

    private static string NormalizeCode(string code) =>
        DomainGuard.OptionalText(code, 30, "código da política")?.ToUpperInvariant()
        ?? throw new DomainException("O código da política é obrigatório.");

    private static string NormalizeVersion(string version) =>
        DomainGuard.OptionalText(version, 20, "versão") ?? throw new DomainException("A versão é obrigatória.");
}

