namespace Fix.Domain.AggregateRoots.Rules;

public static class RuleCodes
{
    /// <summary>Rule de plataforma: ignora as checagens de permissão por organização.</summary>
    public const string SuperAdministrador = "super_administrador";

    public const string Administrador = "administrador";
    public const string Founder = "founder";

    /// <summary>Diretoria: aprova mandatos (inclusive exceções), boletas e a política.</summary>
    public const string Gestor = "gestor";

    /// <summary>Mesa de execução / comercial / logística: propõe mandatos e registra boletas (sem alçada).</summary>
    public const string Operador = "operador";

    /// <summary>Controle de riscos: reconcilia confirmations.</summary>
    public const string MiddleOffice = "middle_office";

    /// <summary>Somente leitura.</summary>
    public const string User = "user";
}

