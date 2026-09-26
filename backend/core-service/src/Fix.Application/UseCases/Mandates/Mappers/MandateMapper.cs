using Fix.Application.Mandates.Commands;
using Fix.Application.Mandates.Dtos;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Application.Mandates.Mappers;

internal static class MandateMapper
{
    public static MandateDto ToDto(this Mandate mandate, Policy? policy, decimal consumed)
    {
        var axis = policy?.Axes.FirstOrDefault(a => a.Id == mandate.AxisId);

        return new MandateDto(
            mandate.Id,
            mandate.Code,
            mandate.PolicyId,
            policy?.Code ?? string.Empty,
            policy?.Version ?? string.Empty,
            mandate.AxisId,
            axis?.Code ?? string.Empty,
            axis?.Title.Value ?? string.Empty,
            mandate.Type,
            mandate.Title.Value,
            mandate.Criteria,
            mandate.Commodity,
            mandate.Tenor?.Code,
            mandate.Quantity,
            mandate.QuantityUnit,
            consumed,
            mandate.Quantity is { } authorized ? authorized - consumed : null,
            new PriceCriteriaDto(mandate.Price.AtMarket, mandate.Price.Target, mandate.Price.Min, mandate.Price.Max, mandate.Price.Unit),
            mandate.WindowStart,
            mandate.WindowEnd,
            mandate.Compliance.ToDto(),
            mandate.Status,
            mandate.IssuedBy,
            mandate.DecidedBy,
            mandate.DecidedAt,
            mandate.DecisionNote);
    }

    public static ComplianceDto ToDto(this Compliance compliance) => new(compliance.Status, compliance.Reason);

    public static MandateTerms ToTerms(this MandateTermsInput input) => new(
        Title.Create(input.Title),
        input.Criteria,
        input.Commodity,
        string.IsNullOrWhiteSpace(input.Tenor) ? null : Tenor.Create(input.Tenor),
        input.Quantity,
        input.QuantityUnit,
        PriceCriteria.Create(input.AtMarket, input.PriceTarget, input.PriceMin, input.PriceMax, input.PriceUnit),
        input.WindowStart,
        input.WindowEnd);
}

