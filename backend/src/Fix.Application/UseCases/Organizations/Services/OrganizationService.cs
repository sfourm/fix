using Fix.Application.Abstractions.Authentication;
using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Organizations.Commands;
using Fix.Application.Organizations.Dtos;
using Fix.Application.Organizations.Mappers;
using Fix.Application.Organizations.Queries;
using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Organizations.Repositories;
using Fix.Domain.AggregateRoots.Rules.Repositories;

namespace Fix.Application.Organizations.Services;

internal sealed class OrganizationService(
    IOrganizationRepository organizationRepository,
    IRuleRepository ruleRepository,
    IIdentityService identityService,
    IRoleResolver roleResolver,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrganizationCommand, OrganizationDto>,
      ICommandHandler<UpdateOrganizationCommand, OrganizationDto>,
      ICommandHandler<UpdateCompanyProfileCommand, OrganizationSetupDto>,
      ICommandHandler<UpdateIndustrialProfileCommand, OrganizationSetupDto>,
      ICommandHandler<UpdateBudgetCommand, OrganizationSetupDto>,
      ICommandHandler<UpdateFinancialsCommand, OrganizationSetupDto>,
      ICommandHandler<AddCommodityCommand, OrganizationSetupDto>,
      ICommandHandler<UpdateCommodityCommand, OrganizationSetupDto>,
      ICommandHandler<RemoveCommodityCommand, OrganizationSetupDto>,
      ICommandHandler<AddMemberCommand, Guid>,
      ICommandHandler<ChangeMemberDeskCommand, Unit>,
      ICommandHandler<RemoveMemberCommand, Unit>,
      ICommandHandler<CreateGroupCommand, Guid>,
      ICommandHandler<AddGroupMemberCommand, Unit>,
      IQueryHandler<GetOrganizationQuery, OrganizationSetupDto>,
      IQueryHandler<ListUserOrganizationsQuery, IReadOnlyList<OrganizationDto>>,
      IQueryHandler<GetUserRolesQuery, IReadOnlyCollection<string>>,
      IQueryHandler<ListMembersQuery, IReadOnlyList<MemberDto>>,
      IQueryHandler<ListGroupsQuery, IReadOnlyList<GroupDto>>
{
    // ---------- Commands: organização ----------

    public async Task<OrganizationDto> HandleAsync(CreateOrganizationCommand command, CancellationToken cancellationToken)
    {
        _ = await identityService.FindByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("Usuário", command.UserId);

        var organization = Organization.Create(Name.Create(command.Name), command.UserId);
        if (await organizationRepository.SlugExistsAsync(organization.Slug, cancellationToken))
        {
            throw new ConflictException($"Já existe uma organização com o identificador '{organization.Slug}'.");
        }

        organizationRepository.Add(organization);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return organization.ToDto();
    }

    public async Task<OrganizationDto> HandleAsync(UpdateOrganizationCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        organization.Rename(Name.Create(command.Name));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return organization.ToDto();
    }

    // ---------- Commands: setup ----------

    public Task<OrganizationSetupDto> HandleAsync(UpdateCompanyProfileCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.UpdateProfile(CompanyProfile.Create(
            command.CorporateName,
            command.TaxId,
            command.Headquarters,
            command.Group,
            command.Sector,
            command.CropYearStartMonth,
            command.ActiveCrop)), cancellationToken);

    public Task<OrganizationSetupDto> HandleAsync(UpdateIndustrialProfileCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.UpdateIndustrial(IndustrialProfile.Create(
            command.MillingCapacity,
            command.MixMinPct,
            command.MixMaxPct,
            command.MixGuidancePct)), cancellationToken);

    public Task<OrganizationSetupDto> HandleAsync(UpdateBudgetCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.UpdateBudget(Budget.Create(
            command.CashCost,
            command.EconomicFloor,
            command.EquivalentPrice,
            command.TargetMarginPct)), cancellationToken);

    public Task<OrganizationSetupDto> HandleAsync(UpdateFinancialsCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.UpdateFinancials(Financials.Create(
            command.Cash,
            command.CreditLines,
            command.MonthlyFixedCost,
            command.NetDebt,
            command.Ebitda,
            command.UsdDebt,
            command.ReferenceDate)), cancellationToken);

    public Task<OrganizationSetupDto> HandleAsync(AddCommodityCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.AddCommodity(
            command.Commodity,
            command.Capacity,
            command.Unit,
            command.PriceReference,
            command.Currency,
            command.Sells), cancellationToken);

    public Task<OrganizationSetupDto> HandleAsync(UpdateCommodityCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.UpdateCommodity(
            command.CommodityId,
            command.Capacity,
            command.Unit,
            command.PriceReference,
            command.Currency,
            command.Sells), cancellationToken);

    public Task<OrganizationSetupDto> HandleAsync(RemoveCommodityCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.RemoveCommodity(command.CommodityId), cancellationToken);

    // ---------- Commands: membros e grupos ----------

    public async Task<Guid> HandleAsync(AddMemberCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        var user = await identityService.FindByEmailAsync(command.Email.Trim(), cancellationToken)
            ?? throw new NotFoundException("Usuário", command.Email);

        var rule = await ruleRepository.GetByCodeAsync(command.RuleCode, cancellationToken)
            ?? throw new NotFoundException("Rule", command.RuleCode);

        var member = organization.AddMember(user.Id, rule.Id, command.Desk);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return member.Id;
    }

    public async Task<Unit> HandleAsync(ChangeMemberDeskCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        organization.ChangeMemberDesk(command.MemberId, command.Desk);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    public async Task<Unit> HandleAsync(RemoveMemberCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        organization.RemoveMember(command.MemberId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    public async Task<Guid> HandleAsync(CreateGroupCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        var rules = (await ruleRepository.ListAsync(cancellationToken)).ToDictionary(r => r.Code);
        var ruleIds = command.RuleCodes
            .Select(code => rules.TryGetValue(code, out var rule) ? rule.Id : throw new NotFoundException("Rule", code))
            .ToList();

        var group = organization.CreateGroup(Name.Create(command.Name), ruleIds);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return group.Id;
    }

    public async Task<Unit> HandleAsync(AddGroupMemberCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        organization.AddMemberToGroup(command.GroupId, command.MemberId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    // ---------- Queries ----------

    public async Task<OrganizationSetupDto> HandleAsync(GetOrganizationQuery query, CancellationToken cancellationToken) =>
        (await GetAsync(query.OrganizationId, cancellationToken)).ToSetupDto();

    public async Task<IReadOnlyList<OrganizationDto>> HandleAsync(
        ListUserOrganizationsQuery query,
        CancellationToken cancellationToken)
    {
        var organizations = await organizationRepository.ListByUserAsync(query.UserId, cancellationToken);
        return organizations.Select(o => o.ToDto()).ToList();
    }

    public async Task<IReadOnlyCollection<string>> HandleAsync(GetUserRolesQuery query, CancellationToken cancellationToken)
    {
        var roles = await roleResolver.GetRolesAsync(query.OrganizationId, query.UserId, cancellationToken);
        return roles.Order().ToList();
    }

    public async Task<IReadOnlyList<MemberDto>> HandleAsync(ListMembersQuery query, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(query.OrganizationId, cancellationToken);

        var users = (await identityService.GetUsersAsync(
                organization.Members.Select(m => m.UserId).ToList(),
                cancellationToken))
            .ToDictionary(u => u.Id);

        return organization.ToMemberDtos(users, await GetRuleCodesAsync(cancellationToken));
    }

    public async Task<IReadOnlyList<GroupDto>> HandleAsync(ListGroupsQuery query, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(query.OrganizationId, cancellationToken);
        return organization.ToGroupDtos(await GetRuleCodesAsync(cancellationToken));
    }

    // ---------- Helpers ----------

    private async Task<OrganizationSetupDto> ChangeSetupAsync(
        Guid organizationId,
        Action<Organization> change,
        CancellationToken cancellationToken)
    {
        var organization = await GetAsync(organizationId, cancellationToken);

        change(organization);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return organization.ToSetupDto();
    }

    private async Task<Organization> GetAsync(Guid organizationId, CancellationToken cancellationToken) =>
        await organizationRepository.GetByIdAsync(organizationId, cancellationToken)
        ?? throw new NotFoundException("Organização", organizationId);

    private async Task<IReadOnlyDictionary<Guid, string>> GetRuleCodesAsync(CancellationToken cancellationToken) =>
        (await ruleRepository.ListAsync(cancellationToken)).ToDictionary(r => r.Id, r => r.Code);
}

