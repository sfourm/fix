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
    public const string DefaultAdministratorsGroupName = "Administradores";

    private static readonly Guid FounderRuleId = SystemRules.Id(RuleCodes.Founder);
    private static readonly Guid AdministradorRuleId = SystemRules.Id(RuleCodes.Administrador);
    private static readonly Guid SuperAdministradorRuleId = SystemRules.Id(RuleCodes.SuperAdministrador);

    private readonly List<OrganizationMember> _members = [];
    private readonly List<OrganizationGroup> _groups = [];
    private readonly List<OrganizationRule> _rules = [];
    private readonly List<OrganizationCommodity> _commodities = [];

    private Organization()
    {
    }

    private Organization(Name name)
    {
        Name = name;
        Slug = Slug.From(name.Value);
        Profile = CompanyProfile.Default(name);
        Industrial = IndustrialProfile.Empty();
        Budget = Budget.Empty();
        Financials = Financials.Empty();
    }

    public Name Name { get; private set; } = null!;

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
    /// Cria a organização: o criador vira membro founder (Diretoria) e é adicionado ao grupo default de administradores.
    /// </summary>
    public static Organization Create(Name name, Guid founderUserId)
    {
        var organization = new Organization(name);

        var founder = organization.AddMember(founderUserId, FounderRuleId, Desk.Board);

        var administrators = organization.CreateGroup(
            Name.Create(DefaultAdministratorsGroupName),
            [AdministradorRuleId],
            isDefault: true);
        organization.AddMemberToGroup(administrators.Id, founder.Id);

        organization.Raise(new OrganizationCreatedDomainEvent(organization.Id, founderUserId));
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

    public OrganizationMember AddMember(Guid userId, Guid ruleId, Desk? desk = null)
    {
        if (IsMember(userId))
        {
            throw new DomainException("O usuário já é membro da organização.");
        }

        var member = new OrganizationMember(Id, userId, desk);
        _members.Add(member);
        AssignRuleToMember(member.Id, ruleId);
        return member;
    }

    public void ChangeMemberDesk(Guid memberId, Desk? desk) => GetMember(memberId).ChangeDesk(desk);

    public void RemoveMember(Guid memberId)
    {
        var member = GetMember(memberId);
        if (IsFounder(member.Id))
        {
            throw new DomainException("O membro founder não pode ser removido da organização.");
        }

        foreach (var group in _groups)
        {
            group.RemoveMember(member.Id);
        }

        _rules.RemoveAll(r => r.MemberId == member.Id);
        _members.Remove(member);
    }

    public OrganizationGroup CreateGroup(Name name, IEnumerable<Guid> ruleIds, bool isDefault = false)
    {
        if (_groups.Any(g => g.Name == name))
        {
            throw new DomainException($"Já existe um grupo com o nome '{name}'.");
        }

        var group = new OrganizationGroup(Id, name, isDefault);
        _groups.Add(group);

        foreach (var ruleId in ruleIds.Distinct())
        {
            AssignRuleToGroup(group.Id, ruleId);
        }

        return group;
    }

    public void AddMemberToGroup(Guid groupId, Guid memberId)
    {
        var group = GetGroup(groupId);
        GetMember(memberId);
        group.AddMember(memberId);
    }

    public void AssignRuleToMember(Guid memberId, Guid ruleId)
    {
        EnsureAssignable(ruleId);
        GetMember(memberId);

        if (ruleId == FounderRuleId && _rules.Any(r => r.RuleId == FounderRuleId))
        {
            throw new DomainException("A organização já possui um founder.");
        }

        if (_rules.Any(r => r.MemberId == memberId && r.RuleId == ruleId))
        {
            return;
        }

        _rules.Add(OrganizationRule.ForMember(Id, ruleId, memberId));
    }

    public void AssignRuleToGroup(Guid groupId, Guid ruleId)
    {
        EnsureAssignable(ruleId);
        GetGroup(groupId);

        if (ruleId == FounderRuleId)
        {
            throw new DomainException("A rule founder só pode ser atribuída diretamente a um membro.");
        }

        if (_rules.Any(r => r.GroupId == groupId && r.RuleId == ruleId))
        {
            return;
        }

        _rules.Add(OrganizationRule.ForGroup(Id, ruleId, groupId));
    }

    public bool IsFounder(Guid memberId) =>
        _rules.Any(r => r.MemberId == memberId && r.RuleId == FounderRuleId);

    private static void EnsureAssignable(Guid ruleId)
    {
        if (ruleId == SuperAdministradorRuleId)
        {
            throw new DomainException(
                "A rule super_administrador é de plataforma e não pode ser atribuída dentro de uma organização.");
        }
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

