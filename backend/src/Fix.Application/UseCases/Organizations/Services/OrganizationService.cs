using Fix.Application.Abstractions.Authentication;
using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Authorization;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Organizations.Commands;
using Fix.Application.Organizations.Dtos;
using Fix.Application.Organizations.Mappers;
using Fix.Application.Organizations.Queries;
using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Organizations.Repositories;
using Fix.Domain.AggregateRoots.Roles;
using Fix.Domain.AggregateRoots.Rules;
using Fix.Domain.AggregateRoots.Rules.Repositories;
using Fix.Domain.Common;

namespace Fix.Application.Organizations.Services;

internal sealed class OrganizationService(
    IOrganizationRepository organizationRepository,
    IRuleRepository ruleRepository,
    IIdentityService identityService,
    IRoleResolver roleResolver,
    InternalAccess internalAccess,
    IUnitOfWork unitOfWork)
    : IOrganizationService
{
    // ---------- Commands: organização ----------

    public async Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationCommand command, CancellationToken cancellationToken)
    {
        _ = await identityService.FindByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException("Usuário", command.UserId);

        var organization = Organization.Create(Name.Create(command.Name), command.UserId);
        if (await organizationRepository.SlugExistsAsync(organization.Slug, cancellationToken))
        {
            throw new ConflictException($"Já existe uma organização com o identificador '{organization.Slug}'.");
        }

        organizationRepository.Add(organization);

        // Toda organização cliente começa com as alçadas-modelo (Gestor, Operador, Middle office), que ela pode editar.
        foreach (var template in SystemRules.Templates)
        {
            ruleRepository.Add(Rule.CreateCustom(
                organization.Id,
                template.Name,
                template.Roles.Select(SystemRoles.Id).ToList(),
                template.Code));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return organization.ToDto();
    }

    public async Task<OrganizationDto> UpdateOrganizationAsync(UpdateOrganizationCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        organization.Rename(Name.Create(command.Name));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return organization.ToDto();
    }

    // ---------- Commands: setup ----------

    public Task<OrganizationSetupDto> UpdateCompanyProfileAsync(UpdateCompanyProfileCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.UpdateProfile(CompanyProfile.Create(
            command.CorporateName,
            command.TaxId,
            command.Headquarters,
            command.Group,
            command.Sector,
            command.CropYearStartMonth,
            command.ActiveCrop)), cancellationToken);

    public Task<OrganizationSetupDto> UpdateIndustrialProfileAsync(UpdateIndustrialProfileCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.UpdateIndustrial(IndustrialProfile.Create(
            command.MillingCapacity,
            command.MixMinPct,
            command.MixMaxPct,
            command.MixGuidancePct)), cancellationToken);

    public Task<OrganizationSetupDto> UpdateBudgetAsync(UpdateBudgetCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.UpdateBudget(Budget.Create(
            command.CashCost,
            command.EconomicFloor,
            command.EquivalentPrice,
            command.TargetMarginPct)), cancellationToken);

    public Task<OrganizationSetupDto> UpdateFinancialsAsync(UpdateFinancialsCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.UpdateFinancials(Financials.Create(
            command.Cash,
            command.CreditLines,
            command.MonthlyFixedCost,
            command.NetDebt,
            command.Ebitda,
            command.UsdDebt,
            command.ReferenceDate)), cancellationToken);

    public Task<OrganizationSetupDto> AddCommodityAsync(AddCommodityCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.AddCommodity(
            command.Commodity,
            command.Capacity,
            command.Unit,
            command.PriceReference,
            command.Currency,
            command.Sells), cancellationToken);

    public Task<OrganizationSetupDto> UpdateCommodityAsync(UpdateCommodityCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.UpdateCommodity(
            command.CommodityId,
            command.Capacity,
            command.Unit,
            command.PriceReference,
            command.Currency,
            command.Sells), cancellationToken);

    public Task<OrganizationSetupDto> RemoveCommodityAsync(RemoveCommodityCommand command, CancellationToken cancellationToken) =>
        ChangeSetupAsync(command.OrganizationId, o => o.RemoveCommodity(command.CommodityId), cancellationToken);

    // ---------- Commands: membros e grupos ----------

    public async Task<Guid> AddMemberAsync(AddMemberCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        var user = await identityService.FindByEmailAsync(command.Email.Trim(), cancellationToken)
            ?? throw new NotFoundException("Usuário", command.Email);

        var member = organization.AddMember(user.Id, command.Desk);

        var code = command.RuleCode?.Trim();
        if (!string.IsNullOrEmpty(code) && code != RuleCodes.User && !organization.IsInternal)
        {
            organization.SetMemberAlcadas(member.Id, await GetAlcadasAsync(organization.Id, [code], cancellationToken));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return member.Id;
    }

    public async Task SetMemberAlcadasAsync(SetMemberAlcadasCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        organization.SetMemberAlcadas(command.MemberId, await GetAlcadasAsync(organization.Id, command.RuleCodes, cancellationToken));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SetGroupAlcadasAsync(SetGroupAlcadasCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        organization.SetGroupAlcadas(command.GroupId, await GetAlcadasAsync(organization.Id, command.RuleCodes, cancellationToken));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Só o owner atual (ou a equipe interna FIX, em suporte) passa a propriedade adiante.</summary>
    public async Task TransferOwnershipAsync(TransferOwnershipCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        var isOwner = organization.Owner?.UserId == command.UserId;
        if (!isOwner && await internalAccess.GetLevelAsync(command.UserId, cancellationToken) == InternalLevel.None)
        {
            throw new ForbiddenException("Só o owner atual pode transferir a propriedade da organização.");
        }

        organization.TransferOwnership(command.MemberId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeMemberDeskAsync(ChangeMemberDeskCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        organization.ChangeMemberDesk(command.MemberId, command.Desk);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMemberAsync(RemoveMemberCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        organization.RemoveMember(command.MemberId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> CreateGroupAsync(CreateGroupCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        var alcadas = await GetAlcadasAsync(organization.Id, command.RuleCodes, cancellationToken);
        var group = organization.CreateGroup(Name.Create(command.Name), alcadas, command.ParentGroupId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return group.Id;
    }

    /// <summary>A própria organização mantém o organograma: reposiciona o grupo (e seus subgrupos) sob outro pai.</summary>
    public async Task MoveGroupAsync(MoveGroupCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        organization.MoveGroup(command.GroupId, command.ParentGroupId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddGroupMemberAsync(AddGroupMemberCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(command.OrganizationId, cancellationToken);

        organization.AddMemberToGroup(command.GroupId, command.MemberId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ---------- Queries ----------

    public async Task<OrganizationSetupDto> GetOrganizationAsync(GetOrganizationQuery query, CancellationToken cancellationToken) =>
        (await GetAsync(query.OrganizationId, cancellationToken)).ToSetupDto();

    public async Task<IReadOnlyList<OrganizationDto>> ListUserOrganizationsAsync(ListUserOrganizationsQuery query, CancellationToken cancellationToken)
    {
        var organizations = await organizationRepository.ListByUserAsync(query.UserId, cancellationToken);
        var result = organizations.Select(o => o.ToDto()).ToList();

        // Equipe interna FIX: vê também todas as organizações clientes, para dar suporte.
        if (await internalAccess.GetLevelAsync(query.UserId, cancellationToken) != InternalLevel.None)
        {
            var memberOf = result.Select(o => o.Id).ToHashSet();
            result.AddRange((await organizationRepository.ListAllAsync(cancellationToken))
                .Where(o => !memberOf.Contains(o.Id) && !o.IsInternal)
                .Select(o => o.ToDto() with { InternalAccess = true }));
        }

        return result;
    }

    public async Task<IReadOnlyCollection<string>> GetUserRolesAsync(GetUserRolesQuery query, CancellationToken cancellationToken)
    {
        var roles = await roleResolver.GetRolesAsync(query.OrganizationId, query.UserId, cancellationToken);
        return roles.Order().ToList();
    }

    public async Task<IReadOnlyList<MemberDto>> ListMembersAsync(ListMembersQuery query, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(query.OrganizationId, cancellationToken);

        var users = (await identityService.GetUsersAsync(
                organization.Members.Select(m => m.UserId).ToList(),
                cancellationToken))
            .ToDictionary(u => u.Id);

        return organization.ToMemberDtos(users, await GetRuleCodesAsync(organization.Id, cancellationToken));
    }

    public async Task<IReadOnlyList<GroupDto>> ListGroupsAsync(ListGroupsQuery query, CancellationToken cancellationToken)
    {
        var organization = await GetAsync(query.OrganizationId, cancellationToken);
        return organization.ToGroupDtos(await GetRuleCodesAsync(organization.Id, cancellationToken));
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

    private async Task<IReadOnlyDictionary<Guid, string>> GetRuleCodesAsync(Guid organizationId, CancellationToken cancellationToken) =>
        (await ruleRepository.ListForOrganizationAsync(organizationId, cancellationToken)).ToDictionary(r => r.Id, r => r.Code);

    /// <summary>Alçadas da organização pelos códigos; código desconhecido é erro (não se atribui o que não existe).</summary>
    private async Task<IReadOnlyList<Rule>> GetAlcadasAsync(Guid organizationId, IReadOnlyCollection<string> codes, CancellationToken cancellationToken)
    {
        var distinct = codes.Select(c => c.Trim()).Where(c => c.Length > 0).Distinct().ToList();
        var alcadas = await ruleRepository.ListAlcadasByCodeAsync(organizationId, distinct, cancellationToken);

        var missing = distinct.Except(alcadas.Select(r => r.Code)).ToList();
        if (missing.Count > 0)
        {
            throw new NotFoundException("Alçada", string.Join(", ", missing));
        }

        return alcadas;
    }
}

