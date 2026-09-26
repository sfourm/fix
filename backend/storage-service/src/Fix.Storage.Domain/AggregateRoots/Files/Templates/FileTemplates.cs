using System.Globalization;
using System.Text;

namespace Fix.Storage.Domain.AggregateRoots.Files.Templates;

/// <summary>
/// Modelos por tipo. Usuários, políticas, mandatos e boletas têm colunas fixas (o cabeçalho é conferido no envio);
/// documentos aceitam qualquer arquivo e só são armazenados.
/// </summary>
public static class FileTemplates
{
    public static readonly IReadOnlyList<string> TabularExtensions = [".csv", ".xlsx", ".xml"];

    private static TemplateColumn Required(string name, string description, string example) => new(name, true, description, example);

    private static TemplateColumn Optional(string name, string description, string example) => new(name, false, description, example);

    private static readonly Dictionary<FileKind, FileTemplate> All = new()
    {
        [FileKind.Users] = new(FileKind.Users, TabularExtensions,
        [
            Required("email", "E-mail de uma conta já cadastrada na plataforma", "ana@empresa.com.br"),
            Optional("cargo", "Cargo inicial (nome ou código)", "Operador (Mesa)"),
            Optional("mesa", "execucao, comercial, logistica, diretoria ou controle_riscos", "execucao"),
            Optional("grupo", "Grupo do organograma (nome)", "Mesa de execução"),
        ]),
        [FileKind.Mandates] = new(FileKind.Mandates, TabularExtensions,
        [
            Required("politica", "Código da política", "POL-2026"),
            Required("eixo", "Código do eixo da política", "POL-PRE"),
            Required("tipo", "precificacao, moeda, comercial ou logistica", "precificacao"),
            Required("titulo", "Direcionamento do mandato", "Fixar 30% da tela N27"),
            Optional("criterio", "Critério livre (ritmo, percentil)", "percentil 60"),
            Optional("commodity", "acucar_vhp, acucar_branco, etanol_hidratado, etanol_anidro, milho ou soja", "acucar_vhp"),
            Optional("tela", "Vencimento (código ICE ou mês/ano)", "N27"),
            Optional("quantidade", "Volume autorizado", "800"),
            Optional("unidade", "lotes, toneladas, sacas, m3, libras ou usd", "lotes"),
            Optional("a_mercado", "sim ou não", "não"),
            Optional("preco_alvo", "Preço target", "16,80"),
            Optional("preco_min", "Preço mínimo", "15,00"),
            Optional("preco_max", "Preço máximo", "18,50"),
            Optional("unidade_preco", "Unidade do preço", "c/lb"),
            Optional("janela_inicio", "Início da janela (dd/mm/aaaa)", "01/10/2026"),
            Optional("janela_fim", "Fim da janela (dd/mm/aaaa)", "31/03/2027"),
        ]),
        [FileKind.Orders] = new(FileKind.Orders, TabularExtensions,
        [
            Optional("mandato", "Código do mandato (vazio = boleta sem mandato, exige justificativa)", "MD-01"),
            Required("contraparte", "Código ou nome da contraparte homologada", "CP-01"),
            Required("instrumento", "futuro, opcao ou ndf", "futuro"),
            Required("operacao", "compra ou venda", "venda"),
            Required("tela", "Vencimento (código ICE ou mês/ano)", "N27"),
            Optional("lotes", "Lotes (futuro e opção)", "150"),
            Optional("nocional_usd", "Nocional em US$ (NDF)", ""),
            Required("preco", "Preço, taxa (NDF) ou strike (opção)", "16,42"),
            Optional("unidade_preco", "Unidade do preço", "c/lb"),
            Optional("tipo_opcao", "call ou put", ""),
            Optional("premio", "Prêmio da opção", ""),
            Required("data_operacao", "Data da operação (dd/mm/aaaa)", "25/09/2026"),
            Optional("commodity", "Só sem mandato", ""),
            Optional("venda_coberta", "sim ou não (venda de opção)", ""),
            Optional("justificativa", "Obrigatória quando há desvio", ""),
            Optional("observacoes", "Observações", ""),
        ]),
        // Só criação: cada linha cria uma política nova (rascunho); alterações seguem pela tela, com versionamento.
        [FileKind.Policies] = new(FileKind.Policies, TabularExtensions,
        [
            Required("codigo", "Código da nova política", "POL-2027"),
            Required("titulo", "Título da política", "Política de hedge 2027"),
            Required("versao", "Versão inicial", "v1"),
            Optional("descricao", "Descrição", "Safra 27/28"),
            Required("vigencia_inicio", "Início da vigência (dd/mm/aaaa)", "01/04/2027"),
            Optional("vigencia_fim", "Fim da vigência (dd/mm/aaaa)", "31/03/2028"),
            Optional("usar_modelo", "sim = já nasce com os eixos, instrumentos e bandas do modelo FIX", "sim"),
        ]),
        [FileKind.Documents] = new(FileKind.Documents, [], []),
    };

    /// <summary>Nomes de coluna que as pessoas costumam escrever por extenso (já normalizados) → nome do modelo.</summary>
    private static readonly Dictionary<string, string> Aliases = new()
    {
        ["e_mail"] = "email",
        ["politica_codigo"] = "politica",
        ["inicio_vigencia"] = "vigencia_inicio",
        ["fim_vigencia"] = "vigencia_fim",
        ["versao_inicial"] = "versao",
        ["preco_minimo"] = "preco_min",
        ["preco_maximo"] = "preco_max",
        ["inicio_janela"] = "janela_inicio",
        ["fim_janela"] = "janela_fim",
        ["data_da_operacao"] = "data_operacao",
        ["nocional"] = "nocional_usd",
        ["observacao"] = "observacoes",
    };

    public static FileTemplate For(FileKind kind) => All[kind];

    /// <summary>Confere a extensão do arquivo contra o modelo (tipos só armazenados aceitam qualquer uma).</summary>
    public static string EnsureExtension(FileKind kind, string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        var template = For(kind);
        if (template.Extensions.Count > 0 && !template.Extensions.Contains(extension))
        {
            throw new DomainException(
                $"Arquivo de {kind.Label()} precisa ser {string.Join(", ", template.Extensions)} (recebido: {(extension == string.Empty ? "sem extensão" : extension)}).");
        }

        return extension;
    }

    /// <summary>
    /// Confere o cabeçalho: todas as colunas obrigatórias presentes e nenhuma desconhecida. Devolve o cabeçalho
    /// normalizado, na ordem do arquivo.
    /// </summary>
    public static IReadOnlyList<string> EnsureHeader(FileKind kind, IReadOnlyList<string> header)
    {
        var template = For(kind);
        var normalized = header.Select(Normalize).Select(h => Aliases.GetValueOrDefault(h, h)).ToList();
        var known = template.Columns.Select(c => c.Name).ToHashSet();

        var missing = template.Columns.Where(c => c.Required && !normalized.Contains(c.Name)).Select(c => c.Name).ToList();
        var unknown = normalized.Where(h => h.Length > 0 && !known.Contains(h)).Distinct().ToList();
        var duplicated = normalized.Where(h => h.Length > 0).GroupBy(h => h).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        var problems = new List<string>();
        if (missing.Count > 0) problems.Add($"faltam as colunas obrigatórias {string.Join(", ", missing)}");
        if (unknown.Count > 0) problems.Add($"colunas fora do modelo: {string.Join(", ", unknown)}");
        if (duplicated.Count > 0) problems.Add($"colunas repetidas: {string.Join(", ", duplicated)}");
        if (problems.Count > 0)
        {
            throw new DomainException($"O arquivo não segue o modelo de {kind.Label()}: {string.Join("; ", problems)}. Baixe o modelo na tela de uploads.");
        }

        return normalized;
    }

    /// <summary>"Preço Mín." → "preco_min": sem acento, minúsculo, espaço/hífen/ponto viram "_".</summary>
    public static string Normalize(string header)
    {
        var decomposed = header.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            builder.Append(char.IsLetterOrDigit(c) ? c : '_');
        }

        return string.Join('_', builder.ToString().Split('_', StringSplitOptions.RemoveEmptyEntries));
    }
}
