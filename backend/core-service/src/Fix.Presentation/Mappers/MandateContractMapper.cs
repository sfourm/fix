using Fix.Application.Mandates.Commands;
using Fix.Application.Mandates.Dtos;
using Contract = Fix.Contracts.V1;
using DomainCommon = Fix.Domain.Common;

namespace Fix.Presentation.Mappers;

internal static class MandateContractMapper
{
    public static Contract.Mandate ToContract(this MandateDto mandate)
    {
        var contract = new Contract.Mandate
        {
            Id = mandate.Id.ToString(),
            Code = mandate.Code,
            PolicyId = mandate.PolicyId.ToString(),
            PolicyCode = mandate.PolicyCode,
            PolicyVersion = mandate.PolicyVersion,
            AxisId = mandate.AxisId.ToString(),
            AxisCode = mandate.AxisCode,
            AxisTitle = mandate.AxisTitle,
            Type = mandate.Type.ToContract<Contract.MandateType>(),
            Terms = mandate.ToContractTerms(),
            Consumed = (double)mandate.Consumed,
            Compliance = mandate.Compliance.ToContract(),
            Status = mandate.Status.ToContract<Contract.MandateStatus>(),
            IssuedBy = mandate.IssuedBy.ToString(),
        };

        if (mandate.Balance is { } balance) contract.Balance = (double)balance;
        if (mandate.DecidedBy is { } decidedBy) contract.DecidedBy = decidedBy.ToString();
        if (mandate.DecidedAt is { } decidedAt) contract.DecidedAt = decidedAt.ToContract();
        if (mandate.DecisionNote is { } note) contract.DecisionNote = note;
        return contract;
    }

    public static Contract.Compliance ToContract(this ComplianceDto compliance) => new()
    {
        Status = compliance.Status.ToContract<Contract.ComplianceStatus>(),
        Reason = compliance.Reason,
    };

    public static MandateTermsInput ToInput(this Contract.MandateTerms? terms)
    {
        terms ??= new Contract.MandateTerms();
        var price = terms.Price ?? new Contract.PriceCriteria();

        return new MandateTermsInput(
            terms.Title,
            terms.Criteria.ToOptionalString(terms.HasCriteria),
            terms.Commodity.ToOptionalDomain<DomainCommon.Commodity>("terms.commodity"),
            terms.Tenor.ToOptionalString(terms.HasTenor),
            terms.Quantity.ToOptionalDecimal(terms.HasQuantity),
            terms.QuantityUnit.ToOptionalDomain<DomainCommon.MeasurementUnit>("terms.quantity_unit"),
            price.AtMarket,
            price.Target.ToOptionalDecimal(price.HasTarget),
            price.Min.ToOptionalDecimal(price.HasMin),
            price.Max.ToOptionalDecimal(price.HasMax),
            price.Unit.ToOptionalString(price.HasUnit),
            terms.WindowStart.ToOptionalString(terms.HasWindowStart).ToOptionalDate("terms.window_start"),
            terms.WindowEnd.ToOptionalString(terms.HasWindowEnd).ToOptionalDate("terms.window_end"));
    }

    private static Contract.MandateTerms ToContractTerms(this MandateDto mandate)
    {
        var price = new Contract.PriceCriteria { AtMarket = mandate.Price.AtMarket };
        if (mandate.Price.Target is { } target) price.Target = (double)target;
        if (mandate.Price.Min is { } min) price.Min = (double)min;
        if (mandate.Price.Max is { } max) price.Max = (double)max;
        if (mandate.Price.Unit is { } unit) price.Unit = unit;

        var terms = new Contract.MandateTerms
        {
            Title = mandate.Title,
            Commodity = mandate.Commodity.ToContract<Contract.Commodity>(),
            QuantityUnit = mandate.QuantityUnit.ToContract<Contract.MeasurementUnit>(),
            Price = price,
        };

        if (mandate.Criteria is { } criteria) terms.Criteria = criteria;
        if (mandate.Tenor is { } tenor) terms.Tenor = tenor;
        if (mandate.Quantity is { } quantity) terms.Quantity = (double)quantity;
        if (mandate.WindowStart is { } start) terms.WindowStart = start.ToContract();
        if (mandate.WindowEnd is { } end) terms.WindowEnd = end.ToContract();
        return terms;
    }
}
