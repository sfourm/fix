using Fix.Application.Abstractions.Messaging;
using Fix.Application.Organizations.Commands;
using Fix.Application.Organizations.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using DomainCommon = Fix.Domain.Common;
using DomainOrganizations = Fix.Domain.AggregateRoots.Organizations;

namespace Fix.Presentation.Services;

internal sealed class OrganizationGrpcService(IDispatcher dispatcher) : OrganizationService.OrganizationServiceBase
{
    public override async Task<Organization> CreateOrganization(CreateOrganizationRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new CreateOrganizationCommand(request.Context.ToUserId(), request.Name),
            context.CancellationToken)).ToContract();

    public override async Task<ListOrganizationsResponse> ListUserOrganizations(
        ListUserOrganizationsRequest request,
        ServerCallContext context)
    {
        var organizations = await dispatcher.QueryAsync(
            new ListUserOrganizationsQuery(request.Context.ToUserId()),
            context.CancellationToken);

        return new ListOrganizationsResponse { Organizations = { organizations.Select(o => o.ToContract()) } };
    }

    // ---------- Setup ----------

    public override async Task<OrganizationSetup> GetOrganization(GetOrganizationRequest request, ServerCallContext context) =>
        (await dispatcher.QueryAsync(
            new GetOrganizationQuery(request.Context.ToUserId(), request.Context.ToOrganizationId()),
            context.CancellationToken)).ToContract();

    public override async Task<Organization> UpdateOrganization(UpdateOrganizationRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new UpdateOrganizationCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Name),
            context.CancellationToken)).ToContract();

    public override async Task<OrganizationSetup> UpdateCompanyProfile(UpdateCompanyProfileRequest request, ServerCallContext context)
    {
        var profile = request.Profile ?? new CompanyProfile();
        return (await dispatcher.SendAsync(
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
            context.CancellationToken)).ToContract();
    }

    public override async Task<OrganizationSetup> UpdateIndustrialProfile(
        UpdateIndustrialProfileRequest request,
        ServerCallContext context)
    {
        var industrial = request.Industrial ?? new IndustrialProfile();
        return (await dispatcher.SendAsync(
            new UpdateIndustrialProfileCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                industrial.MillingCapacity.ToOptionalDecimal(industrial.HasMillingCapacity),
                industrial.MixMinPct.ToOptionalDecimal(industrial.HasMixMinPct),
                industrial.MixMaxPct.ToOptionalDecimal(industrial.HasMixMaxPct),
                industrial.MixGuidancePct.ToOptionalDecimal(industrial.HasMixGuidancePct)),
            context.CancellationToken)).ToContract();
    }

    public override async Task<OrganizationSetup> UpdateBudget(UpdateBudgetRequest request, ServerCallContext context)
    {
        var budget = request.Budget ?? new Budget();
        return (await dispatcher.SendAsync(
            new UpdateBudgetCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                budget.CashCost.ToOptionalDecimal(budget.HasCashCost),
                budget.EconomicFloor.ToOptionalDecimal(budget.HasEconomicFloor),
                budget.EquivalentPrice.ToOptionalDecimal(budget.HasEquivalentPrice),
                budget.TargetMarginPct.ToOptionalDecimal(budget.HasTargetMarginPct)),
            context.CancellationToken)).ToContract();
    }

    public override async Task<OrganizationSetup> UpdateFinancials(UpdateFinancialsRequest request, ServerCallContext context)
    {
        var financials = request.Financials ?? new Financials();
        return (await dispatcher.SendAsync(
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
            context.CancellationToken)).ToContract();
    }

    public override async Task<OrganizationSetup> AddCommodity(AddCommodityRequest request, ServerCallContext context)
    {
        var data = request.Data ?? new CommodityInput();
        return (await dispatcher.SendAsync(
            new AddCommodityCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Commodity.ToDomain<DomainCommon.Commodity>("commodity"),
                data.Capacity.ToDecimal(),
                data.Unit.ToDomain<DomainCommon.MeasurementUnit>("data.unit"),
                data.PriceReference.ToOptionalString(data.HasPriceReference),
                data.Currency,
                data.Sells),
            context.CancellationToken)).ToContract();
    }

    public override async Task<OrganizationSetup> UpdateCommodity(UpdateCommodityRequest request, ServerCallContext context)
    {
        var data = request.Data ?? new CommodityInput();
        return (await dispatcher.SendAsync(
            new UpdateCommodityCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.CommodityId.ToGuid("commodity_id"),
                data.Capacity.ToDecimal(),
                data.Unit.ToDomain<DomainCommon.MeasurementUnit>("data.unit"),
                data.PriceReference.ToOptionalString(data.HasPriceReference),
                data.Currency,
                data.Sells),
            context.CancellationToken)).ToContract();
    }

    public override async Task<OrganizationSetup> RemoveCommodity(RemoveCommodityRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new RemoveCommodityCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.CommodityId.ToGuid("commodity_id")),
            context.CancellationToken)).ToContract();

    public override async Task<UserRolesResponse> GetUserRoles(GetUserRolesRequest request, ServerCallContext context)
    {
        var roles = await dispatcher.QueryAsync(
            new GetUserRolesQuery(request.Context.ToUserId(), request.Context.ToOrganizationId()),
            context.CancellationToken);

        return new UserRolesResponse { Roles = { roles } };
    }

    // ---------- Membros e grupos ----------

    public override async Task<ListMembersResponse> ListMembers(ListMembersRequest request, ServerCallContext context)
    {
        var members = await dispatcher.QueryAsync(
            new ListMembersQuery(request.Context.ToUserId(), request.Context.ToOrganizationId()),
            context.CancellationToken);

        return new ListMembersResponse { Members = { members.Select(m => m.ToContract()) } };
    }

    public override async Task<AddMemberResponse> AddMember(AddMemberRequest request, ServerCallContext context)
    {
        var memberId = await dispatcher.SendAsync(
            new AddMemberCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Email,
                request.RuleCode,
                request.Desk.ToOptionalDomain<DomainOrganizations.Desk>("desk")),
            context.CancellationToken);

        return new AddMemberResponse { MemberId = memberId.ToString() };
    }

    public override async Task<Empty> ChangeMemberDesk(ChangeMemberDeskRequest request, ServerCallContext context)
    {
        await dispatcher.SendAsync(
            new ChangeMemberDeskCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.MemberId.ToGuid("member_id"),
                request.Desk.ToOptionalDomain<DomainOrganizations.Desk>("desk")),
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> RemoveMember(RemoveMemberRequest request, ServerCallContext context)
    {
        await dispatcher.SendAsync(
            new RemoveMemberCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.MemberId.ToGuid("member_id")),
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<ListGroupsResponse> ListGroups(ListGroupsRequest request, ServerCallContext context)
    {
        var groups = await dispatcher.QueryAsync(
            new ListGroupsQuery(request.Context.ToUserId(), request.Context.ToOrganizationId()),
            context.CancellationToken);

        return new ListGroupsResponse { Groups = { groups.Select(g => g.ToContract()) } };
    }

    public override async Task<CreateGroupResponse> CreateGroup(CreateGroupRequest request, ServerCallContext context)
    {
        var groupId = await dispatcher.SendAsync(
            new CreateGroupCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Name,
                request.RuleCodes.ToList()),
            context.CancellationToken);

        return new CreateGroupResponse { GroupId = groupId.ToString() };
    }

    public override async Task<Empty> AddGroupMember(AddGroupMemberRequest request, ServerCallContext context)
    {
        await dispatcher.SendAsync(
            new AddGroupMemberCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.GroupId.ToGuid("group_id"),
                request.MemberId.ToGuid("member_id")),
            context.CancellationToken);

        return new Empty();
    }
}

