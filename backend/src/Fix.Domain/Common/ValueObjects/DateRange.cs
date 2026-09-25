using Fix.Domain.Abstractions;

namespace Fix.Domain.Common;

public sealed class DateRange : ValueObject
{
    // Usado pelo EF Core (complex type).
    private DateRange()
    {
    }

    private DateRange(DateOnly startsOn, DateOnly? endsOn)
    {
        StartsOn = startsOn;
        EndsOn = endsOn;
    }

    public DateOnly StartsOn { get; private set; }

    public DateOnly? EndsOn { get; private set; }

    public static DateRange Create(DateOnly startsOn, DateOnly? endsOn)
    {
        if (endsOn is not null && endsOn < startsOn)
        {
            throw new DomainException("A data final não pode ser anterior à data inicial.");
        }

        return new DateRange(startsOn, endsOn);
    }

    public bool Contains(DateOnly date) => date >= StartsOn && (EndsOn is null || date <= EndsOn);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartsOn;
        yield return EndsOn;
    }
}
