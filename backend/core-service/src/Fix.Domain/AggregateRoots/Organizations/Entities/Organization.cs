using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Rules;

namespace Fix.Domain.AggregateRoots.Organizations;

/// <summary>
/// Companhia (tenant). Raiz do agregado que controla o setup (perfil, capacidade, orçamento, financeiro),
/// as commodities operadas, os membros, os grupos e as atribuições de rules.
/// </summary>
public sealed class Organization : AggregateRoot
{
    /// <summary>Grupo raiz do organograma de uma organização cliente (onde entra o owner).</summary>
    public const string RootGroupName = "Direção";

    /// <summary>Organização nativa da FIX: equipe interna (super administrador e administradores).</summary>
    public const string InternalOrganizationName = "FIX";
    public const string InternalRootGroupName = "Equipe FIX";
    public static readonly Guid InternalOrganizationId = DeterministicGuid.From("organization:fix");

    private readonly List<OrganizationMember> _members = [];
    private readonly List<OrganizationGroup> _groups = [];
    private readonly List<OrganizationRule> _rules = [];
    private readonly List<OrganizationCommodity> _commodities = [];

    private Organization()
    {
    }

    private Organization(Name name)
        : this(Guid.CreateVersion7(), name, isInternal: false)
    {
    }

    private Organization(Guid id, Name name, bool isInternal)
        : base(id)
    {
        IsInternal = isInternal;
        Name = name;
        Slug = Slug.From(name.Value);
        Profile = CompanyProfile.Default(name);
        Industrial = IndustrialProfile.Empty();
        Budget = Budget.Empty();
        Financials = Financials.Empty();
    }

    public Name Name { get; private set; } = null!;

    /// <summary>Organização da própria FIX (equipe interna), e não uma cliente.</summary>
    public bool IsInternal { get; private set; }

    public Slug Slug { get; private set; } = null!;

    public CompanyProfile Profile { get; private set; } = null!;

    public IndustrialProfile Industrial { get; private set; } = null!;

    public Budget Budget { get; private set; } = null!;

    public Financials Financials { get; private set; } = null!;

    public IReadOnlyCollection<OrganizationMember> Members => _members.AsReadOnly();

    public IReadOnlyCollection<OrganizationGroup> Groups => _groups.AsReadOnly();

    public IReadOnlyCollection<OrganizationRule> Rules => _rules.AsReadOnly();

    public IReadOnlyCollection<OrganizationCommodity> Commodities => _commodities.AsReadOnly();

    /// <summary>
    /// Cria uma organização cliente: o criador vira o owner (Diretoria) e entra no grupo raiz do organograma.
    /// As alçadas iniciais (modelos) são criadas pela aplicação a partir de <see cref="SystemRules.Templates"/>.
    /// </summary>
    public static Organization Create(Name name, Guid ownerUserId)
    {
        var organization = new Organization(name);

        var owner = organization.AddMemberWithBase(ownerUserId, SystemRules.OwnerId, Desk.Board);
        var root = organization.AddGroup(Name.Create(RootGroupName), [], isDefault: true, parentGroupId: null);
        organization.AddMemberToGroup(root.Id, owner.Id);

        organization.Raise(new OrganizationCreatedDomainEvent(organization.Id, ownerUserId));
        return organization;
    }

    /// <summary>Cria a organização nativa da FIX com o super administrador (feito uma única vez, na inicialização).</summary>
    public static Organization CreateInternal(Guid superAdministratorUserId)
    {
        var organization = new Organization(InternalOrganizationId, Name.Create(InternalOrganizationName), isInternal: true);

        var superAdministrator = organization.AddMemberWithBase(superAdministratorUserId, SystemRules.SuperAdministradorId, desk: null);
        var root = organization.AddGroup(Name.Create(InternalRootGroupName), [], isDefault: true, parentGroupId: null);
        organization.AddMemberToGroup(root.Id, superAdministrator.Id);
        return organization;
    }

    // ---------- Setup ----------

    public void Rename(Name name) => Name = name;

    public void UpdateProfile(CompanyProfile profile) => Profile = profile;

    public void UpdateIndustrial(IndustrialProfile industrial) => Industrial = industrial;

    public void UpdateBudget(Budget budget) => Budget = budget;

    public void UpdateFinancials(Financials financials) => Financials = financials;

    public OrganizationCommodity AddCommodity(
        Commodity commodity,
        decimal capacity,
        MeasurementUnit unit,
        string? priceReference,
        string currency,
        bool sells)
    {
        if (_commodities.Any(c => c.Commodity == commodity))
        {
            throw new DomainException("Esta commodity já está cadastrada na companhia.");
        }

        var entry = new OrganizationCommodity(Id, commodity);
        entry.Update(capacity, unit, priceReference, currency, sells);
        _commodities.Add(entry);
        return entry;
    }

    public void UpdateCommodity(
        Guid commodityId,
        decimal capacity,
        MeasurementUnit unit,
        string? priceReference,
        string currency,
        bool sells) =>
        GetCommodity(commodityId).Update(capacity, unit, priceReference, currency, sells);

    public void RemoveCommodity(Guid commodityId) => _commodities.Remove(GetCommodity(commodityId));

    public bool Operates(Commodity commodity) => _commodities.Any(c => c.Commodity == commodity);

    // ---------- Membros, grupos e rules ----------

    public bool IsMember(Guid userId) => _members.Any(m => m.UserId == userId);

    /// <summary>
    /// Adiciona um membro: numa organização cliente ele entra como user (permissões extras vêm das alçadas);
    /// na organização FIX, como administrador interno.
    /// </summary>
    public OrganizationMember AddMember(Guid userId, Desk? desk = null) =>
        AddMemberWithBase(userId, IsInternal ? SystemRules.AdministradorId : SystemRules.UserId, desk);

    private OrganizationMember AddMemberWithBase(Guid userId, Guid baseRuleId, Desk? desk)
    {
        if (IsMember(userId))
        {
            throw new DomainException("O usuário já é membro da organização.");
        }

        var member = new OrganizationMember(Id, userId, desk);
        _members.Add(member);
        _rules.Add(OrganizationRule.ForMember(Id, baseRuleId, member.Id));
        return member;
    }

    /// <summary>Membro owner (sempre exatamente um numa organização cliente).</summary>
    public OrganizationMember? Owner =>
        _rules.FirstOrDefault(r => r.RuleId == SystemRules.OwnerId && r.MemberId is not null) is { } rule
            ? _members.First(m => m.Id == rule.MemberId)
            : null;

    public bool IsOwner(Guid memberId) => _rules.Any(r => r.MemberId == memberId && r.RuleId == SystemRules.OwnerId);

    /// <summary>Passa a propriedade para outro membro: ele vira owner e o owner atual vira user.</summary>
    public void TransferOwnership(Guid newOwnerMemberId)
    {
        if (IsInternal)
        {
            throw new DomainException("A organização FIX não tem owner: é gerida pelo super administrador.");
        }

        var newOwner = GetMember(newOwnerMemberId);
        var current = Owner ?? throw new DomainException("A organização está sem owner.");
        if (current.Id == newOwner.Id)
        {
            throw new DomainException("Este membro já é o owner da organização.");
        }

        _rules.RemoveAll(r => r.MemberId == current.Id && r.RuleId == SystemRules.OwnerId);
        _rules.Add(OrganizationRule.ForMember(Id, SystemRules.UserId, current.Id));
        _rules.RemoveAll(r => r.MemberId == newOwner.Id && r.RuleId == SystemRules.UserId);
        _rules.Add(OrganizationRule.ForMember(Id, SystemRules.OwnerId, newOwner.Id));
    }

    public void ChangeMemberDesk(Guid memberId, Desk? desk) => GetMember(memberId).ChangeDesk(desk);

    public void RemoveMember(Guid memberId)
    {
        var member = GetMember(memberId);
        if (IsOwner(member.Id))
        {
            throw new DomainException("O owner não pode ser removido: transfira a propriedade antes.");
        }

        if (_rules.Any(r => r.MemberId == member.Id && r.RuleId == SystemRules.SuperAdministradorId))
        {
            throw new DomainException("O super administrador não pode ser removido da organização FIX.");
        }

        foreach (var group in _groups)
        {
            group.RemoveMember(member.Id);
        }

        _rules.RemoveAll(r => r.MemberId == member.Id);
        _members.Remove(member);
    }

    // ---------- Organograma ----------

    /// <summary>Grupo raiz do organograma (o grupo default, onde está o owner).</summary>
    public OrganizationGroup RootGroup => _groups.First(g => g.ParentGroupId is null);

    /// <summary>Cria um grupo no organograma, abaixo do grupo pai informado (ou da raiz, se não informado).</summary>
    public OrganizationGroup CreateGroup(Name name, IEnumerable<Rule> alcadas, Guid? parentGroupId = null)
    {
        var parent = parentGroupId is { } id ? GetGroup(id) : RootGroup;
        var group = AddGroup(name, [], isDefault: false, parent.Id);
        SetGroupAlcadas(group.Id, alcadas);
        return group;
    }

    /// <summary>
    /// Reposiciona o grupo no organograma, levando junto os grupos abaixo dele. A raiz não se move e um grupo
    /// não pode ficar abaixo de si mesmo nem de um grupo que está abaixo dele (o organograma continua uma árvore).
    /// </summary>
    public void MoveGroup(Guid groupId, Guid parentGroupId)
    {
        var group = GetGroup(groupId);
        var parent = GetGroup(parentGroupId);

        if (group.ParentGroupId is null)
        {
            throw new DomainException($"O grupo '{group.Name}' é a raiz do organograma e não pode ficar abaixo de outro grupo.");
        }

        if (parent.Id == group.Id || AncestorsOf(parent.Id).Contains(group.Id))
        {
            throw new DomainException($"O grupo '{group.Name}' não pode ficar abaixo de '{parent.Name}', que está abaixo dele no organograma.");
        }

        group.MoveUnder(parent.Id);
    }

    /// <summary>Profundidade do grupo no organograma (raiz = 0).</summary>
    public int DepthOf(Guid groupId) => AncestorsOf(GetGroup(groupId).Id).Count;

    /// <summary>
    /// Quem aprova precisa estar num grupo acima de quem pediu: algum grupo do aprovador é ancestral
    /// (estritamente acima) de algum grupo do solicitante. Solicitante sem grupo fica na base do organograma,
    /// então qualquer membro que esteja em algum grupo está acima dele. Ninguém está acima de si mesmo.
    /// </summary>
    public bool IsAboveInOrgChart(Guid approverUserId, Guid requesterUserId)
    {
        if (approverUserId == requesterUserId)
        {
            return false;
        }

        var approverGroups = GroupsOfUser(approverUserId);
        if (approverGroups.Count == 0)
        {
            return false;
        }

        var requesterGroups = GroupsOfUser(requesterUserId);
        if (requesterGroups.Count == 0)
        {
            return true;
        }

        return requesterGroups.Any(group => AncestorsOf(group).Overlaps(approverGroups));
    }

    private OrganizationGroup AddGroup(Name name, IEnumerable<Guid> ruleIds, bool isDefault, Guid? parentGroupId)
    {
        if (_groups.Any(g => g.Name == name))
        {
            throw new DomainException($"Já existe um grupo com o nome '{name}'.");
        }

        var group = new OrganizationGroup(Id, name, isDefault, parentGroupId);
        _groups.Add(group);

        foreach (var ruleId in ruleIds.Distinct())
        {
            _rules.Add(OrganizationRule.ForGroup(Id, ruleId, group.Id));
        }

        return group;
    }

    /// <summary>Grupos acima do grupo informado, do pai até a raiz.</summary>
    private HashSet<Guid> AncestorsOf(Guid groupId)
    {
        var ancestors = new HashSet<Guid>();
        var current = _groups.First(g => g.Id == groupId).ParentGroupId;
        while (current is { } parentId && ancestors.Add(parentId))
        {
            current = _groups.FirstOrDefault(g => g.Id == parentId)?.ParentGroupId;
        }

        return ancestors;
    }

    private HashSet<Guid> GroupsOfUser(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        return member is null
            ? []
            : _groups.Where(g => g.Members.Any(gm => gm.MemberId == member.Id)).Select(g => g.Id).ToHashSet();
    }

    public void AddMemberToGroup(Guid groupId, Guid memberId)
    {
        var group = GetGroup(groupId);
        GetMember(memberId);
        group.AddMember(memberId);
    }

    // ---------- Alçadas personalizadas ----------

    /// <summary>Alçadas do membro (sem contar a rule de base owner/user e as herdadas dos grupos).</summary>
    public IReadOnlyList<Guid> AlcadasOfMember(Guid memberId) =>
        _rules.Where(r => r.MemberId == memberId && !SystemRules.BaseIds.Contains(r.RuleId)).Select(r => r.RuleId).ToList();

    /// <summary>Substitui as alçadas atribuídas diretamente ao membro (a rule de base não muda).</summary>
    public void SetMemberAlcadas(Guid memberId, IEnumerable<Rule> alcadas)
    {
        GetMember(memberId);
        var ids = EnsureAlcadas(alcadas);

        _rules.RemoveAll(r => r.MemberId == memberId && !SystemRules.BaseIds.Contains(r.RuleId) && !ids.Contains(r.RuleId));
        foreach (var id in ids.Where(id => !_rules.Any(r => r.MemberId == memberId && r.RuleId == id)))
        {
            _rules.Add(OrganizationRule.ForMember(Id, id, memberId));
        }
    }

    /// <summary>Substitui as alçadas do grupo: todos os membros do grupo recebem as roles delas.</summary>
    public void SetGroupAlcadas(Guid groupId, IEnumerable<Rule> alcadas)
    {
        GetGroup(groupId);
        var ids = EnsureAlcadas(alcadas);

        _rules.RemoveAll(r => r.GroupId == groupId && !ids.Contains(r.RuleId));
        foreach (var id in ids.Where(id => !_rules.Any(r => r.GroupId == groupId && r.RuleId == id)))
        {
            _rules.Add(OrganizationRule.ForGroup(Id, id, groupId));
        }
    }

    /// <summary>Quantos membros e grupos usam a alçada (para avisar antes de excluí-la).</summary>
    public int UsagesOf(Guid ruleId) => _rules.Count(r => r.RuleId == ruleId);

    /// <summary>Tira a alçada de todos os membros e grupos (a alçada vai ser excluída).</summary>
    public void RevokeAlcada(Guid ruleId)
    {
        if (!SystemRules.BaseIds.Contains(ruleId))
        {
            _rules.RemoveAll(r => r.RuleId == ruleId);
        }
    }

    /// <summary>Só alçadas personalizadas desta organização podem ser atribuídas (nunca owner/user nem as internas).</summary>
    private HashSet<Guid> EnsureAlcadas(IEnumerable<Rule> alcadas)
    {
        var list = alcadas.ToList();
        if (list.Count > 0 && IsInternal)
        {
            throw new DomainException("A organização FIX não usa alçadas: os papéis internos são fixos.");
        }

        foreach (var rule in list)
        {
            if (rule.IsSystem || rule.OrganizationId != Id)
            {
                throw new DomainException($"'{rule.Name}' não é uma alçada desta organização.");
            }
        }

        return list.Select(r => r.Id).ToHashSet();
    }

    private OrganizationMember GetMember(Guid memberId) =>
        _members.FirstOrDefault(m => m.Id == memberId)
        ?? throw new DomainException("Membro não encontrado na organização.");

    private OrganizationGroup GetGroup(Guid groupId) =>
        _groups.FirstOrDefault(g => g.Id == groupId)
        ?? throw new DomainException("Grupo não encontrado na organização.");

    private OrganizationCommodity GetCommodity(Guid commodityId) =>
        _commodities.FirstOrDefault(c => c.Id == commodityId)
        ?? throw new DomainException("Commodity não encontrada na companhia.");
}

