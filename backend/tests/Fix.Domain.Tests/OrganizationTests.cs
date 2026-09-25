using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Rules;

namespace Fix.Domain.Tests;

public sealed class OrganizationTests
{
    private static readonly Guid FounderRuleId = SystemRules.Id(RuleCodes.Founder);
    private static readonly Guid AdministradorRuleId = SystemRules.Id(RuleCodes.Administrador);
    private static readonly Guid UserRuleId = SystemRules.Id(RuleCodes.User);

    [Fact]
    public void Create_binds_creator_as_founder_member()
    {
        var userId = Guid.NewGuid();

        var organization = Organization.Create(Name.Create("Acme Ltda"), userId);

        var founder = Assert.Single(organization.Members);
        Assert.Equal(userId, founder.UserId);
        Assert.True(organization.IsFounder(founder.Id));
        Assert.Contains(organization.Rules, r => r.MemberId == founder.Id && r.RuleId == FounderRuleId);
    }

    [Fact]
    public void Create_generates_default_administrators_group_with_founder()
    {
        var organization = Organization.Create(Name.Create("Acme Ltda"), Guid.NewGuid());
        var founder = organization.Members.Single();

        var group = Assert.Single(organization.Groups);
        Assert.True(group.IsDefault);
        Assert.Equal(Organization.DefaultAdministratorsGroupName, group.Name.Value);
        Assert.Contains(group.Members, m => m.MemberId == founder.Id);
        Assert.Contains(organization.Rules, r => r.GroupId == group.Id && r.RuleId == AdministradorRuleId);
    }

    [Fact]
    public void Create_raises_organization_created_event_and_generates_slug()
    {
        var userId = Guid.NewGuid();

        var organization = Organization.Create(Name.Create("Ação & Cia"), userId);

        Assert.Equal("acao-cia", organization.Slug.Value);
        var domainEvent = Assert.IsType<OrganizationCreatedDomainEvent>(Assert.Single(organization.DomainEvents));
        Assert.Equal(organization.Id, domainEvent.OrganizationId);
        Assert.Equal(userId, domainEvent.FounderUserId);
    }

    [Fact]
    public void Founder_cannot_be_removed()
    {
        var organization = Organization.Create(Name.Create("Acme"), Guid.NewGuid());

        Assert.Throws<DomainException>(() => organization.RemoveMember(organization.Members.Single().Id));
    }

    [Fact]
    public void Removing_member_also_removes_group_memberships_and_roles()
    {
        var organization = Organization.Create(Name.Create("Acme"), Guid.NewGuid());
        var member = organization.AddMember(Guid.NewGuid(), UserRuleId);
        var group = organization.Groups.Single();
        organization.AddMemberToGroup(group.Id, member.Id);

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

        Assert.Throws<DomainException>(() => organization.AddMember(userId, UserRuleId));
    }

    [Fact]
    public void Only_one_founder_and_no_platform_roles_inside_organization()
    {
        var organization = Organization.Create(Name.Create("Acme"), Guid.NewGuid());

        Assert.Throws<DomainException>(() => organization.AddMember(Guid.NewGuid(), FounderRuleId));
        Assert.Throws<DomainException>(() =>
            organization.AddMember(Guid.NewGuid(), SystemRules.Id(RuleCodes.SuperAdministrador)));
    }
}
