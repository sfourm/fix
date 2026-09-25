using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Roles;
using Fix.Domain.AggregateRoots.Rules;

namespace Fix.Domain.Tests;

public sealed class OrganizationTests
{
    private static Rule Alcada(Organization organization, string name, params string[] roles) =>
        Rule.CreateCustom(organization.Id, name, roles.Select(SystemRoles.Id).ToList());

    [Fact]
    public void Create_binds_creator_as_owner_in_root_group()
    {
        var userId = Guid.NewGuid();

        var organization = Organization.Create(Name.Create("Acme Ltda"), userId);

        var owner = Assert.Single(organization.Members);
        Assert.Equal(userId, owner.UserId);
        Assert.True(organization.IsOwner(owner.Id));
        Assert.Equal(owner.Id, organization.Owner?.Id);

        var root = Assert.Single(organization.Groups);
        Assert.True(root.IsDefault);
        Assert.Equal(Organization.RootGroupName, root.Name.Value);
        Assert.Contains(root.Members, m => m.MemberId == owner.Id);
        Assert.DoesNotContain(organization.Rules, r => r.GroupId == root.Id);
        Assert.False(organization.IsInternal);
    }

    [Fact]
    public void Create_raises_organization_created_event_and_generates_slug()
    {
        var userId = Guid.NewGuid();

        var organization = Organization.Create(Name.Create("Ação & Cia"), userId);

        Assert.Equal("acao-cia", organization.Slug.Value);
        var domainEvent = Assert.IsType<OrganizationCreatedDomainEvent>(Assert.Single(organization.DomainEvents));
        Assert.Equal(organization.Id, domainEvent.OrganizationId);
        Assert.Equal(userId, domainEvent.OwnerUserId);
    }

    [Fact]
    public void New_members_are_users_and_owner_cannot_be_removed()
    {
        var organization = Organization.Create(Name.Create("Acme"), Guid.NewGuid());
        var member = organization.AddMember(Guid.NewGuid());

        Assert.Contains(organization.Rules, r => r.MemberId == member.Id && r.RuleId == SystemRules.UserId);
        Assert.False(organization.IsOwner(member.Id));
        Assert.Throws<DomainException>(() => organization.RemoveMember(organization.Owner!.Id));
    }

    [Fact]
    public void Removing_member_also_removes_group_memberships_and_rules()
    {
        var organization = Organization.Create(Name.Create("Acme"), Guid.NewGuid());
        var member = organization.AddMember(Guid.NewGuid());
        var group = organization.Groups.Single();
        organization.AddMemberToGroup(group.Id, member.Id);
        organization.SetMemberAlcadas(member.Id, [Alcada(organization, "Mesa", RoleCodes.CreateOrder)]);

        organization.RemoveMember(member.Id);

        Assert.DoesNotContain(organization.Members, m => m.Id == member.Id);
        Assert.DoesNotContain(group.Members, m => m.MemberId == member.Id);
        Assert.DoesNotContain(organization.Rules, r => r.MemberId == member.Id);
    }

    [Fact]
    public void Same_user_cannot_join_twice()
    {
        var userId = Guid.NewGuid();
        var organization = Organization.Create(Name.Create("Acme"), userId);

        Assert.Throws<DomainException>(() => organization.AddMember(userId));
    }

    [Fact]
    public void Ownership_is_transferred_and_previous_owner_becomes_user()
    {
        var organization = Organization.Create(Name.Create("Acme"), Guid.NewGuid());
        var previous = organization.Owner!;
        var next = organization.AddMember(Guid.NewGuid());

        organization.TransferOwnership(next.Id);

        Assert.Equal(next.Id, organization.Owner!.Id);
        Assert.Single(organization.Rules, r => r.RuleId == SystemRules.OwnerId);
        Assert.Contains(organization.Rules, r => r.MemberId == previous.Id && r.RuleId == SystemRules.UserId);
        Assert.DoesNotContain(organization.Rules, r => r.MemberId == next.Id && r.RuleId == SystemRules.UserId);
        Assert.Throws<DomainException>(() => organization.TransferOwnership(next.Id));
    }

    [Fact]
    public void Only_own_custom_alcadas_can_be_assigned()
    {
        var organization = Organization.Create(Name.Create("Acme"), Guid.NewGuid());
        var other = Organization.Create(Name.Create("Outra"), Guid.NewGuid());
        var member = organization.AddMember(Guid.NewGuid());
        var systemRule = new Rule(SystemRules.OwnerId, RuleCodes.Owner, "Owner");

        Assert.Throws<DomainException>(() => organization.SetMemberAlcadas(member.Id, [systemRule]));
        Assert.Throws<DomainException>(() => organization.SetMemberAlcadas(member.Id, [Alcada(other, "Mesa", RoleCodes.CreateOrder)]));

        var mesa = Alcada(organization, "Mesa", RoleCodes.CreateOrder);
        var middle = Alcada(organization, "Middle", RoleCodes.ManageConfirmation);
        organization.SetMemberAlcadas(member.Id, [mesa, middle]);
        organization.SetMemberAlcadas(member.Id, [middle]);

        Assert.Equal([middle.Id], organization.AlcadasOfMember(member.Id));
        Assert.Contains(organization.Rules, r => r.MemberId == member.Id && r.RuleId == SystemRules.UserId);
    }

    [Fact]
    public void Deleting_an_alcada_revokes_it_everywhere()
    {
        var organization = Organization.Create(Name.Create("Acme"), Guid.NewGuid());
        var mesa = Alcada(organization, "Mesa", RoleCodes.CreateOrder);
        var member = organization.AddMember(Guid.NewGuid());
        var group = organization.CreateGroup(Name.Create("Mesa de execução"), [mesa]);
        organization.SetMemberAlcadas(member.Id, [mesa]);
        Assert.Equal(2, organization.UsagesOf(mesa.Id));

        organization.RevokeAlcada(mesa.Id);

        Assert.Equal(0, organization.UsagesOf(mesa.Id));
        Assert.DoesNotContain(organization.Rules, r => r.GroupId == group.Id);
    }

    [Fact]
    public void Internal_organization_has_super_administrator_and_admins_without_alcadas()
    {
        var superAdministrator = Guid.NewGuid();
        var fix = Organization.CreateInternal(superAdministrator);

        Assert.True(fix.IsInternal);
        Assert.Equal(Organization.InternalOrganizationId, fix.Id);
        Assert.Null(fix.Owner);
        var root = Assert.Single(fix.Members);
        Assert.Contains(fix.Rules, r => r.MemberId == root.Id && r.RuleId == SystemRules.SuperAdministradorId);

        var admin = fix.AddMember(Guid.NewGuid());
        Assert.Contains(fix.Rules, r => r.MemberId == admin.Id && r.RuleId == SystemRules.AdministradorId);
        Assert.Throws<DomainException>(() => fix.RemoveMember(root.Id));
        Assert.Throws<DomainException>(() => fix.TransferOwnership(admin.Id));
    }

    [Fact]
    public void Custom_alcada_needs_name_and_roles_and_system_rules_are_read_only()
    {
        var organizationId = Guid.NewGuid();

        var rule = Rule.CreateCustom(organizationId, "Mesa Sul", [SystemRoles.Id(RoleCodes.CreateOrder)]);
        Assert.Equal("mesa_sul", rule.Code);
        Assert.False(rule.IsSystem);

        rule.Update("Mesa Sul e Norte", [SystemRoles.Id(RoleCodes.ViewOrder), SystemRoles.Id(RoleCodes.CreateOrder)]);
        Assert.Equal(2, rule.Roles.Count);
        Assert.Equal("mesa_sul", rule.Code);

        Assert.Throws<DomainException>(() => Rule.CreateCustom(organizationId, "Vazia", []));
        Assert.Throws<DomainException>(() => Rule.CreateCustom(organizationId, " ", [SystemRoles.Id(RoleCodes.ViewOrder)]));
        Assert.Throws<DomainException>(() => new Rule(SystemRules.UserId, RuleCodes.User, "Usuário").Update("x", [SystemRoles.Id(RoleCodes.ViewOrder)]));
    }

    [Fact]
    public void Staff_roles_exclude_every_decision()
    {
        Assert.NotEmpty(SystemRoles.Staff);
        Assert.DoesNotContain(SystemRoles.Staff, SystemRoles.Decisions.Contains);
        Assert.Contains(RoleCodes.EditOrganization, SystemRoles.Staff);
    }
}
