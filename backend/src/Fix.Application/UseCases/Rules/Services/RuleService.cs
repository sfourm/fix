using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Rules.Commands;
using Fix.Application.Rules.Dtos;
using Fix.Application.Rules.Mappers;
using Fix.Application.Rules.Queries;
using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Organizations.Repositories;
using Fix.Domain.AggregateRoots.Roles;
using Fix.Domain.AggregateRoots.Rules;
using Fix.Domain.AggregateRoots.Rules.Repositories;

namespace Fix.Application.Rules.Services;

/// <summary>
/// Rules da organização: owner e user são fixas; as alçadas personalizadas são criadas e editadas pela própria
/// organização a partir das roles do sistema. As rules internas da FIX só aparecem na organização FIX.
/// </summary>
internal sealed class RuleService(
    IRuleRepository ruleRepository,
    IOrganizationRepository organizationRepository,
    IUnitOfWork unitOfWork)
    : IRuleService
{
    public async Task<IReadOnlyList<RuleDto>> ListRulesAsync(ListRulesQuery query, CancellationToken cancellationToken)
    {
        var organization = await GetOrganizationAsync(query.OrganizationId, cancellationToken);
        var rules = await ruleRepository.ListForOrganizationAsync(organization.Id, cancellationToken);

        return rules
            .Where(r => SystemRules.IsInternal(r.Code) == organization.IsInternal || !r.IsSystem)
            .Where(r => !organization.IsInternal || r.IsSystem)
            .OrderBy(r => r.IsSystem ? 0 : 1)
            .ThenBy(r => r.Name, StringComparer.CurrentCulture)
            .Select(r => r.ToDto(organization.UsagesOf(r.Id)))
            .ToList();
    }

    public async Task<RuleDto> CreateRuleAsync(CreateRuleCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetCustomerOrganizationAsync(command.OrganizationId, cancellationToken);

        var rule = Rule.CreateCustom(organization.Id, command.Name, RoleIds(command.RoleCodes));
        if (await ruleRepository.CodeExistsAsync(organization.Id, rule.Code, null, cancellationToken))
        {
            throw new ConflictException($"Já existe uma alçada com o nome '{command.Name.Trim()}'.");
        }

        ruleRepository.Add(rule);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return rule.ToDto(usages: 0);
    }

    public async Task<RuleDto> UpdateRuleAsync(UpdateRuleCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetCustomerOrganizationAsync(command.OrganizationId, cancellationToken);
        var rule = await GetAlcadaAsync(organization, command.Id, cancellationToken);

        rule.Update(command.Name, RoleIds(command.RoleCodes));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return rule.ToDto(organization.UsagesOf(rule.Id));
    }

    public async Task DeleteRuleAsync(DeleteRuleCommand command, CancellationToken cancellationToken)
    {
        var organization = await GetCustomerOrganizationAsync(command.OrganizationId, cancellationToken);
        var rule = await GetAlcadaAsync(organization, command.Id, cancellationToken);

        organization.RevokeAlcada(rule.Id);
        ruleRepository.Remove(rule);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static List<Guid> RoleIds(IEnumerable<string> roleCodes) => roleCodes.Distinct().Select(SystemRoles.Id).ToList();

    private async Task<Organization> GetOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
        await organizationRepository.GetByIdAsync(organizationId, cancellationToken)
        ?? throw new NotFoundException("Organização", organizationId);

    private async Task<Organization> GetCustomerOrganizationAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var organization = await GetOrganizationAsync(organizationId, cancellationToken);
        if (organization.IsInternal)
        {
            throw new DomainException("A organização FIX não usa alçadas: os papéis internos são fixos.");
        }

        return organization;
    }

    /// <summary>Só alçadas da própria organização são editáveis (rules de sistema e de outras organizações não).</summary>
    private async Task<Rule> GetAlcadaAsync(Organization organization, Guid ruleId, CancellationToken cancellationToken)
    {
        var rule = await ruleRepository.GetByIdAsync(ruleId, cancellationToken);
        if (rule is null || (!rule.IsSystem && rule.OrganizationId != organization.Id))
        {
            throw new NotFoundException("Alçada", ruleId);
        }

        rule.EnsureCustom();
        return rule;
    }
}
