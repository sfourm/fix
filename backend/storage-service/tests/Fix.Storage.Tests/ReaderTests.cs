using System.Text;
using ClosedXML.Excel;
using Fix.Storage.Infrastructure.Spreadsheets;

namespace Fix.Storage.Tests;

public class ReaderTests
{
    private readonly TableReader reader = new();

    [Fact]
    public void Csv_with_semicolon_quotes_and_blank_lines()
    {
        var csv = "email;cargo;grupo\r\nana@x.com;\"Operador; Mesa\";\"Mesa \"\"A\"\"\"\r\n\r\nbia@x.com;;\n";
        var table = reader.Read(Encoding.UTF8.GetBytes(csv), ".csv");

        Assert.Equal(["email", "cargo", "grupo"], table.Header);
        Assert.Equal(2, table.Rows.Count);
        Assert.Equal(["ana@x.com", "Operador; Mesa", "Mesa \"A\""], table.Rows[0]);
        Assert.Equal(["bia@x.com", "", ""], table.Rows[1]);
    }

    [Fact]
    public void Csv_with_comma_and_bom()
    {
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("email,mesa\nana@x.com,logística\n")).ToArray();
        var table = reader.Read(bytes, ".csv");

        Assert.Equal(["email", "mesa"], table.Header);
        Assert.Equal("logística", table.Rows[0][1]);
    }

    [Fact]
    public void Csv_saved_by_excel_in_latin1_keeps_accents()
    {
        var table = reader.Read(Encoding.Latin1.GetBytes("titulo;preco\nFixação;16,42\n"), ".csv");
        Assert.Equal("Fixação", table.Rows[0][0]);
    }

    [Fact]
    public void Xlsx_numbers_and_dates_come_out_in_pt_br()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Boletas");
        sheet.Cell(1, 1).Value = "preco";
        sheet.Cell(1, 2).Value = "data_operacao";
        sheet.Cell(1, 3).Value = "venda_coberta";
        sheet.Cell(2, 1).Value = 16.42;
        sheet.Cell(2, 2).Value = new DateTime(2026, 9, 25);
        sheet.Cell(2, 3).Value = true;
        using var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);

        var table = reader.Read(stream.ToArray(), ".xlsx");

        Assert.Equal(["preco", "data_operacao", "venda_coberta"], table.Header);
        Assert.Equal(["16,42", "25/09/2026", "sim"], table.Rows[0]);
    }

    [Fact]
    public void Xml_rows_become_columns_by_element_name()
    {
        const string xml = "<linhas><linha><email>ana@x.com</email><mesa>execucao</mesa></linha><linha><email>bia@x.com</email><grupo>Mesa</grupo></linha></linhas>";
        var table = reader.Read(Encoding.UTF8.GetBytes(xml), ".xml");

        Assert.Equal(["email", "mesa", "grupo"], table.Header);
        Assert.Equal(["ana@x.com", "execucao", ""], table.Rows[0]);
        Assert.Equal(["bia@x.com", "", "Mesa"], table.Rows[1]);
    }
}
