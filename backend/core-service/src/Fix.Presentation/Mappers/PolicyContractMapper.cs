using Fix.Application.Policies.Commands;
using Fix.Application.Policies.Dtos;
using Contract = Fix.Contracts.V1;
using Entities = Fix.Domain.AggregateRoots.Policies;

namespace Fix.Presentation.Mappers;

internal static class PolicyContractMapper
{
    public static Contract.PolicySummary ToContract(this PolicySummaryDto policy)
    {
        var contract = new Contract.PolicySummary
        {
            Id = policy.Id.ToString(),
            Code = policy.Code,
            Title = policy.Title,
            Version = policy.Version,
            Status = policy.Status.ToContract<Contract.PolicyStatus>(),
            ValidFrom = policy.ValidFrom.ToContract(),
            AxesCount = policy.AxesCount,
        };

        if (policy.ValidTo is { } validTo) contract.ValidTo = validTo.ToContract();
        return contract;
    }

    public static Contract.Policy ToContract(this PolicyDto policy)
    {
        var contract = new Contract.Policy
        {
            Id = policy.Id.ToString(),
            Code = policy.Code,
            Title = policy.Title,
            Version = policy.Version,
            Status = policy.Status.ToContract<Contract.PolicyStatus>(),
            ValidFrom = policy.ValidFrom.ToContract(),
            Limits = policy.Limits.ToContract(),
            Axes = { policy.Axes.Select(a => a.ToContract()) },
            Bands = { policy.Bands.Select(b => b.ToContract()) },
            Instruments = { policy.Instruments.Select(i => i.ToContract()) },
            Versions = { policy.Versions.Select(v => v.ToContract()) },
        };

        if (policy.Description is { } description) contract.Description = description;
        if (policy.ValidTo is { } validTo) contract.ValidTo = validTo.ToContract();
        if (policy.ApprovalRecord is { } record) contract.ApprovalRecord = record;
        if (policy.ApprovedOn is { } approvedOn) contract.ApprovedOn = approvedOn.ToContract();
        return contract;
    }

    public static PolicyLimitsDto ToDto(this Contract.PolicyLimits? limits)
    {
        limits ??= new Contract.PolicyLimits();
        return new PolicyLimitsDto(
            limits.HedgeHorizonYears,
            limits.AbsoluteCeilingPct.ToDecimal(),
            limits.FxFixedMinPct.ToDecimal(),
            limits.FxFixedMaxPct.ToDecimal(),
            limits.FxUnfixedMaxPct.ToDecimal(),
            limits.MarginCashMaxPct.ToDecimal(),
            limits.PhysicalConcentrationMaxPct.ToDecimal(),
            limits.FinancialConcentrationMaxPct.ToDecimal(),
            limits.LogisticsDeadlineMonths,
            limits.FreightCeilingPct.ToDecimal(),
            limits.CoveredCallMaxPct.ToDecimal());
    }

    public static PolicyAxisInput ToInput(this Contract.PolicyAxisInput? axis)
    {
        axis ??= new Contract.PolicyAxisInput();
        return new PolicyAxisInput(
            axis.Code,
            axis.Title,
            axis.Factor.ToDomain<Entities.RiskFactor>("axis.factor"),
            axis.Statement.ToOptionalString(axis.HasStatement),
            axis.LimitDescription.ToOptionalString(axis.HasLimitDescription),
            axis.Approver.ToOptionalString(axis.HasApprover),
            axis.Restrictions.ToList());
    }

    public static CoverageBandInput ToInput(this Contract.CoverageBandInput? band)
    {
        band ??= new Contract.CoverageBandInput();
        return new CoverageBandInput(
            band.Horizon,
            band.Crop,
            band.MinPct.ToDecimal(),
            band.MaxPct.ToDecimal(),
            band.Note.ToOptionalString(band.HasNote));
    }

    public static PolicyInstrumentInput ToInput(this Contract.PolicyInstrumentInput? instrument)
    {
        instrument ??= new Contract.PolicyInstrumentInput();
        return new PolicyInstrumentInput(
            instrument.Name,
            instrument.Permission.ToDomain<Entities.InstrumentPermission>("instrument.permission"),
            instrument.Condition.ToOptionalString(instrument.HasCondition));
    }

    private static Contract.PolicyLimits ToContract(this PolicyLimitsDto limits) => new()
    {
        HedgeHorizonYears = limits.HedgeHorizonYears,
        AbsoluteCeilingPct = (double)limits.AbsoluteCeilingPct,
        FxFixedMinPct = (double)limits.FxFixedMinPct,
        FxFixedMaxPct = (double)limits.FxFixedMaxPct,
        FxUnfixedMaxPct = (double)limits.FxUnfixedMaxPct,
        MarginCashMaxPct = (double)limits.MarginCashMaxPct,
        PhysicalConcentrationMaxPct = (double)limits.PhysicalConcentrationMaxPct,
        FinancialConcentrationMaxPct = (double)limits.FinancialConcentrationMaxPct,
        LogisticsDeadlineMonths = limits.LogisticsDeadlineMonths,
        FreightCeilingPct = (double)limits.FreightCeilingPct,
        CoveredCallMaxPct = (double)limits.CoveredCallMaxPct,
    };

    private static Contract.PolicyAxis ToContract(this PolicyAxisDto axis)
    {
        var contract = new Contract.PolicyAxis
        {
            Id = axis.Id.ToString(),
            Code = axis.Code,
            Title = axis.Title,
            Factor = axis.Factor.ToContract<Contract.RiskFactor>(),
            Restrictions = { axis.Restrictions },
        };

        if (axis.Statement is { } statement) contract.Statement = statement;
        if (axis.LimitDescription is { } limit) contract.LimitDescription = limit;
        if (axis.Approver is { } approver) contract.Approver = approver;
        return contract;
    }

    private static Contract.CoverageBand ToContract(this CoverageBandDto band)
    {
        var contract = new Contract.CoverageBand
        {
            Id = band.Id.ToString(),
            Horizon = band.Horizon,
            Crop = band.Crop,
            MinPct = (double)band.MinPct,
            MaxPct = (double)band.MaxPct,
        };

        if (band.Note is { } note) contract.Note = note;
        return contract;
    }

    private static Contract.PolicyInstrument ToContract(this PolicyInstrumentDto instrument)
    {
        var contract = new Contract.PolicyInstrument
        {
            Id = instrument.Id.ToString(),
            Name = instrument.Name,
            Permission = instrument.Permission.ToContract<Contract.InstrumentPermission>(),
        };

        if (instrument.Condition is { } condition) contract.Condition = condition;
        return contract;
    }

    private static Contract.PolicyVersion ToContract(this PolicyVersionDto version)
    {
        var contract = new Contract.PolicyVersion
        {
            Version = version.Version,
            Status = version.Status.ToContract<Contract.PolicyStatus>(),
            Date = version.Date.ToContract(),
        };

        if (version.Note is { } note) contract.Note = note;
        return contract;
    }
}

