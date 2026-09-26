using Fix.Storage.Domain.Abstractions;
using Fix.Storage.Domain.AggregateRoots.Files;
using Fix.Storage.Domain.AggregateRoots.Files.Templates;

namespace Fix.Storage.Tests;

public class TemplateTests
{
    [Theory]
    [InlineData("Preço Mín.", "preco_min")]
    [InlineData("  Data da Operação ", "data_da_operacao")]
    [InlineData("E-mail", "e_mail")]
    [InlineData("janela_inicio", "janela_inicio")]
    public void Normalize_removes_accents_and_separators(string header, string expected) =>
        Assert.Equal(expected, FileTemplates.Normalize(header));

    [Fact]
    public void Header_in_any_case_and_with_accents_is_accepted()
    {
        var header = FileTemplates.EnsureHeader(FileKind.Users, ["E-mail", "Cargo", "MESA", "Grupo "]);
        Assert.Equal(["email", "cargo", "mesa", "grupo"], header);
    }

    [Fact]
    public void Missing_required_column_is_rejected()
    {
        var exception = Assert.Throws<DomainException>(() => FileTemplates.EnsureHeader(FileKind.Orders, ["contraparte", "preco"]));
        Assert.Contains("instrumento", exception.Message);
        Assert.Contains("data_operacao", exception.Message);
    }

    [Fact]
    public void Unknown_and_repeated_columns_are_rejected()
    {
        var exception = Assert.Throws<DomainException>(() => FileTemplates.EnsureHeader(FileKind.Users, ["email", "email", "telefone"]));
        Assert.Contains("telefone", exception.Message);
        Assert.Contains("repetidas: email", exception.Message);
    }

    [Fact]
    public void Processed_kinds_only_accept_spreadsheets()
    {
        Assert.Equal(".xlsx", FileTemplates.EnsureExtension(FileKind.Mandates, "Mandatos.XLSX"));
        Assert.Throws<DomainException>(() => FileTemplates.EnsureExtension(FileKind.Mandates, "mandatos.pdf"));
        Assert.Throws<DomainException>(() => FileTemplates.EnsureExtension(FileKind.Policies, "politica.pdf"));
        Assert.Equal(".pdf", FileTemplates.EnsureExtension(FileKind.Documents, "contrato.pdf"));
    }

    [Theory]
    [InlineData(FileKind.Users)]
    [InlineData(FileKind.Policies)]
    [InlineData(FileKind.Mandates)]
    [InlineData(FileKind.Orders)]
    public void Example_csv_follows_its_own_template(FileKind kind)
    {
        var header = FileTemplates.For(kind).ExampleCsv().Split("\r\n")[0].Split(';');
        FileTemplates.EnsureHeader(kind, header);
    }
}
