using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Rules;

/// <summary>
/// Rule: conjunto de roles (permissões atômicas). As de sistema (owner, user e as internas da FIX) não têm organização
/// e não mudam; as alçadas personalizadas pertencem a uma organização, que escolhe nome e roles.
/// </summary>
public sealed class Rule : AggregateRoot
{
    public const int NameMaxLength = 128;

    private readonly List<RuleRole> _roles = [];

    private Rule()
    {
    }

    /// <summary>Rule de sistema (seed).</summary>
    public Rule(Guid id, string code, string name) : base(id)
    {
        Code = code;
        Name = name;
    }

    /// <summary>Organização dona da alçada; null nas rules de sistema.</summary>
    public Guid? OrganizationId { get; private set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public bool IsSystem => OrganizationId is null;

    public IReadOnlyCollection<RuleRole> Roles => _roles.AsReadOnly();

    /// <summary>Cria uma alçada da organização. Sem código explícito, ele é derivado do nome (ex.: "Mesa Sul" → mesa_sul).</summary>
    public static Rule CreateCustom(Guid organizationId, string name, IReadOnlyCollection<Guid> roleIds, string? code = null)
    {
        var rule = new Rule { OrganizationId = organizationId, Code = code ?? CodeFrom(name), Name = ValidName(name) };
        rule.ReplaceRoles(roleIds);
        return rule;
    }

    public static string CodeFrom(string name)
    {
        var code = Slug.From(ValidName(name)).Value.Replace('-', '_');
        return code.Length > 64 ? code[..64].TrimEnd('_') : code;
    }

    /// <summary>Renomeia e troca as roles concedidas. O código não muda (é a referência estável da alçada).</summary>
    public void Update(string name, IReadOnlyCollection<Guid> roleIds)
    {
        EnsureCustom();
        Name = ValidName(name);
        ReplaceRoles(roleIds);
    }

    public void EnsureCustom()
    {
        if (IsSystem)
        {
            throw new DomainException($"A rule '{Name}' é do sistema e não pode ser alterada.");
        }
    }

    public void Grant(Guid roleId)
    {
        if (_roles.Any(r => r.RoleId == roleId))
        {
            return;
        }

        _roles.Add(new RuleRole(Id, roleId));
    }

    private void ReplaceRoles(IReadOnlyCollection<Guid> roleIds)
    {
        if (roleIds.Count == 0)
        {
            throw new DomainException("Uma alçada precisa conceder pelo menos uma role.");
        }

        _roles.RemoveAll(r => !roleIds.Contains(r.RoleId));
        foreach (var roleId in roleIds.Distinct())
        {
            Grant(roleId);
        }
    }

    private static string ValidName(string name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length is 0 or > NameMaxLength)
        {
            throw new DomainException($"O nome da alçada deve ter entre 1 e {NameMaxLength} caracteres.");
        }

        return trimmed;
    }
}
