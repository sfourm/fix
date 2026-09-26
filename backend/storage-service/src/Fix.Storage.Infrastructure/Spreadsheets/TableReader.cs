using System.Globalization;
using System.Text;
using System.Xml.Linq;
using ClosedXML.Excel;
using Fix.Storage.Application.Abstractions.Exceptions;
using Fix.Storage.Application.Abstractions.Spreadsheets;

namespace Fix.Storage.Infrastructure.Spreadsheets;

/// <summary>
/// Lê CSV, XLSX e XML numa tabela de texto. Números e datas do XLSX saem no formato pt-BR ("16,42", "25/09/2026"),
/// igual ao que se digita num CSV, para a conversão das linhas tratar os três formatos do mesmo jeito.
/// </summary>
internal sealed class TableReader : ITableReader
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public Table Read(byte[] content, string extension) => extension switch
    {
        ".csv" => ReadCsv(content),
        ".xlsx" => ReadXlsx(content),
        ".xml" => ReadXml(content),
        _ => throw new BadRequestException($"Formato {extension} não é lido; use .csv, .xlsx ou .xml."),
    };

    public static Table ReadCsv(byte[] content)
    {
        var text = Decode(content);
        var delimiter = DetectDelimiter(text);
        var records = ParseCsv(text, delimiter);
        if (records.Count == 0)
        {
            throw new BadRequestException("O arquivo está vazio.");
        }

        return new Table(records[0], records.Skip(1).ToList());
    }

    public static Table ReadXlsx(byte[] content)
    {
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(new System.IO.MemoryStream(content, writable: false));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new BadRequestException("O arquivo .xlsx não pôde ser aberto (arquivo corrompido ou protegido por senha).");
        }

        using (workbook)
        {
            var sheet = workbook.Worksheets.FirstOrDefault(s => s.Visibility == XLWorksheetVisibility.Visible)
                ?? throw new BadRequestException("A planilha não tem abas.");
            var used = sheet.RangeUsed();
            if (used is null)
            {
                throw new BadRequestException("O arquivo está vazio.");
            }

            var firstColumn = used.FirstColumn().ColumnNumber();
            var lastColumn = used.LastColumn().ColumnNumber();
            var rows = new List<IReadOnlyList<string>>();
            foreach (var row in used.Rows())
            {
                var values = new List<string>(lastColumn - firstColumn + 1);
                for (var column = firstColumn; column <= lastColumn; column++)
                {
                    values.Add(CellText(row.WorksheetRow().Cell(column)));
                }

                rows.Add(values);
            }

            return new Table(rows[0], rows.Skip(1).ToList());
        }
    }

    /// <summary>XML: cada elemento filho da raiz é uma linha; os elementos filhos da linha são as colunas.</summary>
    public static Table ReadXml(byte[] content)
    {
        XDocument document;
        try
        {
            document = XDocument.Load(new System.IO.MemoryStream(content, writable: false));
        }
        catch (System.Xml.XmlException exception)
        {
            throw new BadRequestException($"O XML é inválido: {exception.Message}");
        }

        var rows = document.Root?.Elements().ToList() ?? [];
        var header = new List<string>();
        foreach (var column in rows.SelectMany(r => r.Elements()).Select(e => e.Name.LocalName))
        {
            if (!header.Contains(column))
            {
                header.Add(column);
            }
        }

        if (header.Count == 0)
        {
            throw new BadRequestException("O XML não tem linhas no formato <linhas><linha><coluna>valor</coluna></linha></linhas>.");
        }

        return new Table(
            header,
            [
                .. rows.Select(row => (IReadOnlyList<string>)header
                    .Select(h => row.Elements().FirstOrDefault(e => e.Name.LocalName == h)?.Value ?? string.Empty)
                    .ToList()),
            ]);
    }

    private static string CellText(IXLCell cell)
    {
        var value = cell.Value;
        return value.Type switch
        {
            XLDataType.Blank => string.Empty,
            XLDataType.Number => value.GetNumber().ToString("0.##########", PtBr),
            XLDataType.DateTime => value.GetDateTime().ToString("dd/MM/yyyy", PtBr),
            XLDataType.Boolean => value.GetBoolean() ? "sim" : "não",
            XLDataType.Error => string.Empty,
            _ => cell.GetFormattedString(),
        };
    }

    /// <summary>UTF-8 (com ou sem BOM); se não for UTF-8 válido, Windows-1252/Latin-1 (CSV salvo pelo Excel em pt-BR).</summary>
    private static string Decode(byte[] content)
    {
        try
        {
            return new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(content).TrimStart('﻿');
        }
        catch (DecoderFallbackException)
        {
            return Encoding.Latin1.GetString(content);
        }
    }

    /// <summary>Separador da primeira linha: ";" (Excel pt-BR), "," ou tabulação.</summary>
    private static char DetectDelimiter(string text)
    {
        var counts = new Dictionary<char, int> { [';'] = 0, [','] = 0, ['\t'] = 0 };
        var quoted = false;
        foreach (var c in text)
        {
            if (c == '"') quoted = !quoted;
            else if (!quoted && (c == '\n' || c == '\r')) break;
            else if (!quoted && counts.ContainsKey(c)) counts[c]++;
        }

        var best = counts.MaxBy(p => p.Value);
        return best.Value == 0 ? ';' : best.Key;
    }

    /// <summary>CSV (RFC 4180): campos entre aspas podem ter separador, quebra de linha e aspas duplicadas.</summary>
    private static List<IReadOnlyList<string>> ParseCsv(string text, char delimiter)
    {
        var records = new List<IReadOnlyList<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        var quoted = false;

        void EndField()
        {
            record.Add(field.ToString());
            field.Clear();
        }

        void EndRecord()
        {
            EndField();
            if (record.Count > 1 || record[0].Length > 0)
            {
                records.Add(record);
            }

            record = [];
        }

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    quoted = false;
                }
                else
                {
                    field.Append(c);
                }
            }
            else if (c == '"' && field.Length == 0)
            {
                quoted = true;
            }
            else if (c == delimiter)
            {
                EndField();
            }
            else if (c == '\r' || c == '\n')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                EndRecord();
            }
            else
            {
                field.Append(c);
            }
        }

        if (field.Length > 0 || record.Count > 0)
        {
            EndRecord();
        }

        return records;
    }
}
