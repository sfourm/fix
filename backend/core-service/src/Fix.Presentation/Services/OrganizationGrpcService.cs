using Fix.Application.Abstractions.Validation;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Organizations.Commands;
using Fix.Application.Organizations.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Fix.Presentation.UseCases;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using DomainCommon = Fix.Domain.Common;
using DomainOrganizations = Fix.Domain.AggregateRoots.Organizations;

namespace Fix.Presentation.Services;

internal sealed class OrganizationGrpcService(
    IValidationFactory validation,
    IOrganizationService organizationService) : OrganizationService.OrganizationServiceBase
{
    public override async Task<Organization> CreateOrganization(CreateOrganizationRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new CreateOrganizationCommand(request.Context.ToUserId(), request.Name),
            organizationService.CreateOrganizationAsync,
            context.CancellationToken)).ToContract();

    public override async Task<ListOrganizationsResponse> ListUserOrganizations(
        ListUserOrganizationsRequest request,
        ServerCallContext context)
    {
        var organizations = await validation.RunAsync(
            new ListUserOrganizationsQuery(request.Context.ToUserId()),
            organizationService.ListUserOrganizationsAsync,
            context.CancellationToken);

        return new ListOrganizationsResponse { Organizations = { organizations.Select(o => o.ToContract()) } };
    }

    // ---------- Setup ----------

    public override async Task<OrganizationSetup> GetOrganization(GetOrganizationRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new GetOrganizationQuery(request.Context.ToUserId(), request.Context.ToOrganizationId()),
            organizationService.GetOrganizationAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Organization> UpdateOrganization(UpdateOrganizationRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new UpdateOrganizationCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Name),
            organizationService.UpdateOrganizationAsync,
            context.CancellationToken)).ToContract();

    public override async Task<OrganizationSetup> UpdateCompanyProfile(UpdateCompanyProfileRequest request, ServerCallContext context)
    {
        var profile = request.Profile ?? new CompanyProfile();
        return (await validation.RunAsync(
            new UpdateCompanyProfileCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                profile.CorporateName,
                profile.TaxId.ToOptionalString(profile.HasTaxId),
                profile.Headquarters.ToOptionalString(profile.HasHeadquarters),
                profile.Group.ToOptionalString(profile.HasGroup),
                profile.Sector.ToDomain<DomainOrganizations.Sector>("profile.sector"),
                profile.CropYearStartMonth,
                profile.ActiveCrop.ToOptionalString(profile.HasActiveCrop)),
            organizationService.UpdateCompanyProfileAsync,
            context.CancellationToken)).ToContract();
    }

    public override async Task<OrganizationSetup> UpdateIndustrialProfile(
        UpdateIndustrialProfileRequest request,
        ServerCallContext context)
    {
        var industrial = request.Industrial ?? new IndustrialProfile();
        return (await validation.RunAsync(
            new UpdateIndustrialProfileCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                industrial.MillingCapacity.ToOptionalDecimal(industrial.HasMillingCapacity),
                industrial.MixMinPct.ToOptionalDecimal(industrial.HasMixMinPct),
                industrial.MixMaxPct.ToOptionalDecimal(industrial.HasMixMaxPct),
                industrial.MixGuidancePct.ToOptionalDecimal(industrial.HasMixGuidancePct)),
            organizationService.UpdateIndustrialProfileAsync,
            context.CancellationToken)).ToContract();
    }

    public override async Task<OrganizationSetup> UpdateBudget(UpdateBudgetRequest request, ServerCallContext context)
    {
        var budget = request.Budget ?? new Budget();
        return (await validation.RunAsync(
            new UpdateBudgetCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                budget.CashCost.ToOptionalDecimal(budget.HasCashCost),
                budget.EconomicFloor.ToOptionalDecimal(budget.HasEconomicFloor),
                budget.EquivalentPrice.ToOptionalDecimal(budget.HasEquivalentPrice),
                budget.TargetMarginPct.ToOptionalDecimal(budget.HasTargetMarginPct)),
            organizationService.UpdateBudgetAsync,
            context.CancellationToken)).ToContract();
    }

    public override async Task<OrganizationSetup> UpdateFinancials(UpdateFinancialsRequest request, ServerCallContext context)
    {
        var financials = request.Financials ?? new Financials();
        return (await validation.RunAsync(
            new UpdateFinancialsCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                financials.Cash.ToOptionalDecimal(financials.HasCash),
                financials.CreditLines.ToOptionalDecimal(financials.HasCreditLines),
                financials.MonthlyFixedCost.ToOptionalDecimal(financials.HasMonthlyFixedCost),
                financials.NetDebt.ToOptionalDecimal(financials.HasNetDebt),
                financials.Ebitda.ToOptionalDecimal(financials.HasEbitda),
                financials.UsdDebt.ToOptionalDecimal(financials.HasUsdDebt),
                financials.ReferenceDate.ToOptionalString(financials.HasReferenceDate).ToOptionalDate("financials.reference_date")),
            organizationService.UpdateFinancialsAsync,
            context.CancellationToken)).ToContract();
    }

    public override async Task<OrganizationSetup> AddCommodity(AddCommodityRequest request, ServerCallContext context)
    {
        var data = request.Data ?? new CommodityInput();
        return (await validation.RunAsync(
            new AddCommodityCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Commodity.ToDomain<DomainCommon.Commodity>("commodity"),
                data.Capacity.ToDecimal(),
                data.Unit.ToDomain<DomainCommon.MeasurementUnit>("data.unit"),
                data.PriceReference.ToOptionalString(data.HasPriceReference),
                data.Currency,
                data.Sells),
            organizationService.AddCommodityAsync,
            context.CancellationToken)).ToContract();
    }

    public override async Task<OrganizationSetup> UpdateCommodity(UpdateCommodityRequest request, ServerCallContext context)
    {
        var data = request.Data ?? new CommodityInput();
        return (await validation.RunAsync(
            new UpdateCommodityCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.CommodityId.ToGuid("commodity_id"),
                data.Capacity.ToDecimal(),
                data.Unit.ToDomain<DomainCommon.MeasurementUnit>("data.unit"),
                data.PriceReference.ToOptionalString(data.HasPriceReference),
                data.Currency,
                data.Sells),
            organizationService.UpdateCommodityAsync,
            context.CancellationToken)).ToContract();
    }

    public override async Task<OrganizationSetup> RemoveCommodity(RemoveCommodityRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new RemoveCommodityCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.CommodityId.ToGuid("commodity_id")),
            organizationService.RemoveCommodityAsync,
            context.CancellationToken)).ToContract();

    public override async Task<UserRolesResponse> GetUserRoles(GetUserRolesRequest request, ServerCallContext context)
    {
        var roles = await validation.RunAsync(
            new GetUserRolesQuery(request.Context.ToUserId(), request.Context.ToOrganizationId()),
            organizationService.GetUserRolesAsync,
            context.CancellationToken);

        return new UserRolesResponse { Roles = { roles } };
    }

    // ---------- Membros e grupos ----------

    public override async Task<ListMembersResponse> ListMembers(ListMembersRequest request, ServerCallContext context)
    {
        var members = await validation.RunAsync(
            new ListMembersQuery(request.Context.ToUserId(), request.Context.ToOrganizationId()),
            organizationService.ListMembersAsync,
            context.CancellationToken);

        return new ListMembersResponse { Members = { members.Select(m => m.ToContract()) } };
    }

    public override async Task<AddMemberResponse> AddMember(AddMemberRequest request, ServerCallContext context)
    {
        var memberId = await validation.RunAsync(
            new AddMemberCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Email,
                request.RuleCode,
                request.Desk.ToOptionalDomain<DomainOrganizations.Desk>("desk")),
            organizationService.AddMemberAsync,
            context.CancellationToken);

        return new AddMemberResponse { MemberId = memberId.ToString() };
    }

    public override async Task<Empty> ChangeMemberDesk(ChangeMemberDeskRequest request, ServerCallContext context)
    {
        await validation.ExecuteAsync(
            new ChangeMemberDeskCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.MemberId.ToGuid("member_id"),
                request.Desk.ToOptionalDomain<DomainOrganizations.Desk>("desk")),
            organizationService.ChangeMemberDeskAsync,
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> SetMemberRules(SetMemberRulesRequest request, ServerCallContext context)
    {
        await validation.ExecuteAsync(
            new SetMemberAlcadasCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.MemberId.ToGuid("member_id"),
                request.RuleCodes.ToList()),
            organizationService.SetMemberAlcadasAsync,
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> TransferOwnership(TransferOwnershipRequest request, ServerCallContext context)
    {
        await validation.ExecuteAsync(
            new TransferOwnershipCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.MemberId.ToGuid("member_id")),
            organizationService.TransferOwnershipAsync,
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> SetGroupRules(SetGroupRulesRequest request, ServerCallContext context)
    {
        await validation.ExecuteAsync(
            new SetGroupAlcadasCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.GroupId.ToGuid("group_id"),
                request.RuleCodes.ToList()),
            organizationService.SetGroupAlcadasAsync,
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> RemoveMember(RemoveMemberRequest request, ServerCallContext context)
    {
        await validation.ExecuteAsync(
            new RemoveMemberCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.MemberId.ToGuid("member_id")),
            organizationService.RemoveMemberAsync,
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<ListGroupsResponse> ListGroups(ListGroupsRequest request, ServerCallContext context)
    {
        var groups = await validation.RunAsync(
            new ListGroupsQuery(request.Context.ToUserId(), request.Context.ToOrganizationId()),
            organizationService.ListGroupsAsync,
            context.CancellationToken);

        return new ListGroupsResponse { Groups = { groups.Select(g => g.ToContract()) } };
    }

    public override async Task<CreateGroupResponse> CreateGroup(CreateGroupRequest request, ServerCallContext context)
    {
        var groupId = await validation.RunAsync(
            new CreateGroupCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Name,
                request.RuleCodes.ToList(),
                request.ParentGroupId.ToOptionalString(request.HasParentGroupId).ToOptionalGuid("parent_group_id")),
            organizationService.CreateGroupAsync,
            context.CancellationToken);

        return new CreateGroupResponse { GroupId = groupId.ToString() };
    }

    public override async Task<Empty> MoveGroup(MoveGroupRequest request, ServerCallContext context)
    {
        await validation.ExecuteAsync(
            new MoveGroupCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.GroupId.ToGuid("group_id"),
                request.ParentGroupId.ToGuid("parent_group_id")),
            organizationService.MoveGroupAsync,
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> AddGroupMember(AddGroupMemberRequest request, ServerCallContext context)
    {
        await validation.ExecuteAsync(
            new AddGroupMemberCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.GroupId.ToGuid("group_id"),
                request.MemberId.ToGuid("member_id")),
            organizationService.AddGroupMemberAsync,
            context.CancellationToken);

        return new Empty();
    }
}

