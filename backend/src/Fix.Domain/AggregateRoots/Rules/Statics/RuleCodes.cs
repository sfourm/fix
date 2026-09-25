namespace Fix.Domain.AggregateRoots.Rules;

public static class RuleCodes
{
    // ---------- Internas (equipe FIX, só existem na organização FIX; nunca expostas aos clientes) ----------

    /// <summary>Super administrador da plataforma: controla a equipe interna (administradores).</summary>
    public const string SuperAdministrador = "super_administrador";

    /// <summary>Administrador interno: vê e edita as organizações clientes para dar suporte, sem decidir operações.</summary>
    public const string Administrador = "administrador";

    // ---------- Base de todo membro de organização cliente ----------

    /// <summary>Dono da organização: sempre exatamente um, transferível.</summary>
    public const string Owner = "owner";

    /// <summary>Demais membros. As permissões extras vêm das alçadas personalizadas da organização.</summary>
    public const string User = "user";

    // ---------- Modelos de alçada copiados para cada nova organização (editáveis por ela) ----------

    /// <summary>Diretoria: aprova mandatos (inclusive exceções), boletas e a política.</summary>
    public const string Gestor = "gestor";

    /// <summary>Mesa de execução / comercial / logística: propõe mandatos e registra boletas (sem alçada).</summary>
    public const string Operador = "operador";

    /// <summary>Controle de riscos: reconcilia confirmations.</summary>
    public const string MiddleOffice = "middle_office";
}
