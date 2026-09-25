using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Rules;

namespace Fix.Domain.Tests;

/// <summary>Organograma: grupos em árvore, geridos pela organização, e a regra "aprova quem está acima".</summary>
public sealed class OrgChartTests
{

    private readonly Guid _founder = Guid.NewGuid();
    private readonly Guid _manager = Guid.NewGuid();
    private readonly Guid _trader = Guid.NewGuid();
    private readonly Guid _outsider = Guid.NewGuid();

    /// <summary>Administradores (founder) → Diretoria (gestor) → Mesa (operador); Logística ao lado da Diretoria.</summary>
    private (Organization Organization, OrganizationGroup Board, OrganizationGroup Desk, OrganizationGroup Logistics) Chart()
    {
        var organization = Organization.Create(Name.Create("Usina"), _founder);
        var board = organization.CreateGroup(Name.Create("Diretoria"), []);
        var desk = organization.CreateGroup(Name.Create("Mesa"), [], board.Id);
        var logistics = organization.CreateGroup(Name.Create("Logística"), []);

        organization.AddMemberToGroup(board.Id, organization.AddMember(_manager).Id);
        organization.AddMemberToGroup(desk.Id, organization.AddMember(_trader).Id);
        organization.AddMember(_outsider);
        return (organization, board, desk, logistics);
    }

    [Fact]
    public void Default_group_is_the_single_root_and_new_groups_go_under_it()
    {
        var (organization, board, desk, _) = Chart();

        Assert.True(organization.RootGroup.IsDefault);
        Assert.Null(organization.RootGroup.ParentGroupId);
        Assert.Equal(organization.RootGroup.Id, board.ParentGroupId);
        Assert.Equal(board.Id, desk.ParentGroupId);
        Assert.Equal(2, organization.DepthOf(desk.Id));
    }

    [Fact]
    public void Approver_must_be_in_a_group_above_the_requester()
    {
        var (organization, _, _, _) = Chart();

        Assert.True(organization.IsAboveInOrgChart(_manager, _trader));
        Assert.True(organization.IsAboveInOrgChart(_founder, _trader));
        Assert.False(organization.IsAboveInOrgChart(_trader, _manager));
        Assert.False(organization.IsAboveInOrgChart(_manager, _manager));
    }

    [Fact]
    public void Same_level_or_side_branch_does_not_approve()
    {
        var (organization, board, _, logistics) = Chart();
        var peer = Guid.NewGuid();
        organization.AddMemberToGroup(logistics.Id, organization.AddMember(peer).Id);

        Assert.False(organization.IsAboveInOrgChart(peer, _trader));
        Assert.False(organization.IsAboveInOrgChart(peer, _manager));

        var colleague = Guid.NewGuid();
        organization.AddMemberToGroup(board.Id, organization.AddMember(colleague).Id);
        Assert.False(organization.IsAboveInOrgChart(colleague, _manager));
    }

    [Fact]
    public void Requester_without_group_is_at_the_base_and_approver_without_group_approves_nobody()
    {
        var (organization, _, _, _) = Chart();

        Assert.True(organization.IsAboveInOrgChart(_trader, _outsider));
        Assert.False(organization.IsAboveInOrgChart(_outsider, _trader));
    }

    [Fact]
    public void Moving_a_group_carries_its_subtree_and_changes_who_approves()
    {
        var (organization, _, desk, logistics) = Chart();

        organization.MoveGroup(desk.Id, logistics.Id);

        Assert.Equal(logistics.Id, desk.ParentGroupId);
        Assert.False(organization.IsAboveInOrgChart(_manager, _trader));
    }

    [Fact]
    public void Chart_stays_a_tree()
    {
        var (organization, board, desk, _) = Chart();

        Assert.Throws<DomainException>(() => organization.MoveGroup(board.Id, desk.Id));
        Assert.Throws<DomainException>(() => organization.MoveGroup(board.Id, board.Id));
        Assert.Throws<DomainException>(() => organization.MoveGroup(organization.RootGroup.Id, board.Id));
        Assert.Throws<DomainException>(() => organization.CreateGroup(Name.Create("Órfão"), [], Guid.NewGuid()));
    }

    [Fact]
    public void Removing_a_member_from_a_group_takes_the_position_in_the_org_chart()
    {
        var (organization, _, desk, _) = Chart();
        var traderMember = organization.Members.Single(m => m.UserId == _trader);

        organization.RemoveMemberFromGroup(desk.Id, traderMember.Id);

        Assert.Empty(desk.Members);
        Assert.True(organization.IsMember(_trader));
        Assert.Throws<DomainException>(() => organization.RemoveMemberFromGroup(desk.Id, traderMember.Id));
    }

    [Fact]
    public void Owner_always_stays_in_the_root_group()
    {
        var (organization, _, _, _) = Chart();
        var owner = organization.Owner!;

        Assert.Throws<DomainException>(() => organization.RemoveMemberFromGroup(organization.RootGroup.Id, owner.Id));
    }

    [Fact]
    public void Groups_can_be_renamed_with_a_unique_name()
    {
        var (organization, board, _, _) = Chart();

        organization.RenameGroup(board.Id, Name.Create("Conselho"));

        Assert.Equal("Conselho", board.Name.ToString());
        Assert.Throws<DomainException>(() => organization.RenameGroup(board.Id, Name.Create("Mesa")));
    }

    [Fact]
    public void Deleting_a_group_drops_its_alcadas_but_keeps_the_members()
    {
        var (organization, _, desk, logistics) = Chart();
        var alcada = Rule.CreateCustom(organization.Id, "Frete", [Guid.NewGuid()]);
        organization.SetGroupAlcadas(logistics.Id, [alcada]);
        var driver = Guid.NewGuid();
        organization.AddMemberToGroup(logistics.Id, organization.AddMember(driver).Id);

        organization.DeleteGroup(logistics.Id);
        organization.DeleteGroup(desk.Id);

        Assert.DoesNotContain(organization.Groups, g => g.Id == logistics.Id || g.Id == desk.Id);
        Assert.Equal(0, organization.UsagesOf(alcada.Id));
        Assert.True(organization.IsMember(driver));
    }

    [Fact]
    public void Root_and_groups_with_subgroups_cannot_be_deleted()
    {
        var (organization, board, _, _) = Chart();

        Assert.Throws<DomainException>(() => organization.DeleteGroup(organization.RootGroup.Id));
        Assert.Throws<DomainException>(() => organization.DeleteGroup(board.Id));
    }
}
