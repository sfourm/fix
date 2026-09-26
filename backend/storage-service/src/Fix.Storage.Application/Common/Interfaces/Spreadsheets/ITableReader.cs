namespace Fix.Storage.Application.Abstractions.Spreadsheets;

/// <summary>Planilha lida: cabeçalho e linhas de dados (valores como texto, na ordem do cabeçalho).</summary>
public sealed record Table(IReadOnlyList<string> Header, IReadOnlyList<IReadOnlyList<string>> Rows);

/// <summary>Leitura de CSV, XLSX e XML.</summary>
public interface ITableReader
{
    Table Read(byte[] content, string extension);
}
