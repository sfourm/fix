using Fix.Contracts.V1;
using Fix.Storage.Infrastructure.CoreService;

namespace Fix.Storage.Tests;

public class LineValuesTests
{
    [Theory]
    [InlineData("16,42", 16.42)]
    [InlineData("16.42", 16.42)]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("1,234.56", 1234.56)]
    [InlineData("1.500.000", 1500000)]
    [InlineData("-0,5", -0.5)]
    [InlineData("150", 150)]
    public void Numbers_in_pt_br_and_invariant(string text, double expected) =>
        Assert.Equal(expected, LineValues.ParseNumber(text)!.Value, 6);

    [Fact]
    public void Invalid_number_fails_the_line_with_the_column_name()
    {
        var values = new LineValues(new Dictionary<string, string> { ["preco"] = "dezesseis" });
        var exception = Assert.Throws<LineValueException>(() => values.Number("preco"));
        Assert.Contains("preco", exception.Message);
    }

    [Theory]
    [InlineData("25/09/2026")]
    [InlineData("2026-09-25")]
    [InlineData("25-09-2026")]
    public void Dates_go_to_iso(string text) =>
        Assert.Equal("2026-09-25", new LineValues(new Dictionary<string, string> { ["d"] = text }).Date("d"));

    [Fact]
    public void Options_ignore_accents_and_case()
    {
        var values = new LineValues(new Dictionary<string, string>
        {
            ["mesa"] = "Logística",
            ["tipo"] = "Precificação",
            ["commodity"] = "Açúcar VHP",
            ["a_mercado"] = "Não",
        });

        Assert.Equal(Desk.Logistics, values.Option("mesa", LineValues.Desks));
        Assert.Equal(MandateType.Pricing, values.Option("tipo", LineValues.MandateTypes));
        Assert.Equal(Commodity.RawSugar, values.Option("commodity", LineValues.Commodities));
        Assert.False(values.Flag("a_mercado"));
    }

    [Fact]
    public void Empty_optional_is_null_and_empty_required_fails()
    {
        var values = new LineValues(new Dictionary<string, string> { ["mesa"] = "  ", ["operacao"] = "" });
        Assert.Null(values.Option("mesa", LineValues.Desks));
        Assert.Throws<LineValueException>(() => values.RequiredOption("operacao", LineValues.Directions));
    }
}
