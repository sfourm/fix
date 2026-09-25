using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.Tests;

public sealed class ValueObjectTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Title_is_required(string? value) => Assert.Throws<DomainException>(() => Title.Create(value));

    [Fact]
    public void Title_is_trimmed_and_limited()
    {
        Assert.Equal("abc", Title.Create("  abc ").Value);
        Assert.Throws<DomainException>(() => Title.Create(new string('x', Title.MaxLength + 1)));
    }

    [Fact]
    public void Value_objects_compare_by_value()
    {
        Assert.Equal(Title.Create("abc"), Title.Create("abc"));
        Assert.Equal(Name.Create("Admins"), Name.Create("ADMINS"));
        Assert.Equal(DateRange.Create(new DateOnly(2026, 1, 1), null), DateRange.Create(new DateOnly(2026, 1, 1), null));
    }

    [Theory]
    [InlineData("N26", 2026, 7)]
    [InlineData("h28", 2028, 3)]
    [InlineData("fev/27", 2027, 2)]
    public void Tenor_parses_exchange_codes_and_month_names(string code, int year, int month)
    {
        var tenor = Tenor.Create(code);

        Assert.Equal(new DateOnly(year, month, 1), tenor.Month);
        Assert.Equal(11, Tenor.Create("N27").MonthsFrom(new DateOnly(2026, 8, 14)));
    }

    [Fact]
    public void Tenor_and_crop_year_reject_invalid_values()
    {
        Assert.Throws<DomainException>(() => Tenor.Create("X"));
        Assert.Throws<DomainException>(() => CropYear.Create("26/28"));
        Assert.Equal("26/27", CropYear.Of(new DateOnly(2027, 3, 31), startMonth: 4).Value);
    }

    [Fact]
    public void DateRange_rejects_end_before_start() =>
        Assert.Throws<DomainException>(() => DateRange.Create(new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1)));
}
