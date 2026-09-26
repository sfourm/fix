namespace Fix.Storage.Domain.AggregateRoots.Files.Templates;

/// <summary>Coluna esperada no arquivo.</summary>
public sealed record TemplateColumn(string Name, bool Required, string Description, string Example);

/// <summary>Modelo de um tipo de arquivo: extensões aceitas e colunas (vazio = armazenado sem ler o conteúdo).</summary>
public sealed record FileTemplate(FileKind Kind, IReadOnlyList<string> Extensions, IReadOnlyList<TemplateColumn> Columns)
{
    public bool Processed => Kind.IsProcessed();

    /// <summary>CSV de exemplo (cabeçalho + uma linha), separado por ponto e vírgula como o Excel em pt-BR.</summary>
    public string ExampleCsv() =>
        string.Join(';', Columns.Select(c => c.Name)) + "\r\n" + string.Join(';', Columns.Select(c => c.Example)) + "\r\n";
}
