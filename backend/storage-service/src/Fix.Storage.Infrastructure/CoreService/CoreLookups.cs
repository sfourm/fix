using Fix.Contracts.V1;
using Fix.Storage.Domain.AggregateRoots.Files.Templates;
using Microsoft.Extensions.Caching.Memory;

namespace Fix.Storage.Infrastructure.CoreService;

/// <summary>
/// Consultas ao core para traduzir o que vem na planilha (códigos e nomes) em ids. Ficam em cache por pouco tempo:
/// um arquivo com mil boletas não consulta as contrapartes mil vezes.
/// </summary>
internal sealed class CoreLookups(
    OrganizationService.OrganizationServiceClient organizations,
    RuleService.RuleServiceClient rules,
    PolicyService.PolicyServiceClient policies,
    MandateService.MandateServiceClient mandates,
    CounterpartyService.CounterpartyServiceClient counterparties,
    IMemoryCache cache)
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);
    private const int PageSize = 100;

    public async Task<Rule> RuleAsync(RequestContext context, string value, CancellationToken cancellationToken)
    {
        var all = await CachedAsync(context, "rules", async () =>
            (await rules.ListRulesAsync(new ListRulesRequest { Context = context }, cancellationToken: cancellationToken)).Rules.ToList());
        return all.FirstOrDefault(r => Same(r.Code, value) || Same(r.Name, value))
            ?? throw new LineValueException($"O cargo \"{value}\" não existe na organização.");
    }

    public async Task<Group> GroupAsync(RequestContext context, string name, CancellationToken cancellationToken)
    {
        var all = await CachedAsync(context, "groups", async () =>
            (await organizations.ListGroupsAsync(new ListGroupsRequest { Context = context }, cancellationToken: cancellationToken)).Groups.ToList());
        return all.FirstOrDefault(g => Same(g.Name, name))
            ?? throw new LineValueException($"O grupo \"{name}\" não existe no organograma.");
    }

    /// <summary>Política pelo código: a versão ativa; sem ativa, a mais recente.</summary>
    public async Task<Policy> PolicyAsync(RequestContext context, string code, CancellationToken cancellationToken)
    {
        var all = await CachedAsync(context, "policies", async () =>
        {
            var items = new List<PolicySummary>();
            for (var page = 1; ; page++)
            {
                var response = await policies.ListPoliciesAsync(
                    new ListPoliciesRequest { Context = context, Page = new PageRequest { Page = page, PageSize = PageSize } },
                    cancellationToken: cancellationToken);
                items.AddRange(response.Policies);
                if (response.Policies.Count < PageSize || items.Count >= response.Page.TotalCount) return items;
            }
        });

        var matches = all.Where(p => Same(p.Code, code)).ToList();
        var summary = matches.FirstOrDefault(p => p.Status == PolicyStatus.Active)
            ?? matches.OrderByDescending(p => p.ValidFrom, StringComparer.Ordinal).FirstOrDefault()
            ?? throw new LineValueException($"A política \"{code}\" não existe.");

        return await CachedAsync(context, "policy:" + summary.Id, async () =>
            await policies.GetPolicyAsync(new PolicyIdRequest { Context = context, Id = summary.Id }, cancellationToken: cancellationToken));
    }

    public async Task<Mandate> MandateAsync(RequestContext context, string code, CancellationToken cancellationToken)
    {
        // Sem cache: mandatos criados por uma planilha anterior precisam aparecer já na próxima.
        for (var page = 1; ; page++)
        {
            var response = await mandates.ListMandatesAsync(
                new ListMandatesRequest { Context = context, Page = new PageRequest { Page = page, PageSize = PageSize } },
                cancellationToken: cancellationToken);
            if (response.Mandates.FirstOrDefault(m => Same(m.Code, code)) is { } mandate) return mandate;
            if (response.Mandates.Count < PageSize || page * PageSize >= response.Page.TotalCount)
            {
                throw new LineValueException($"O mandato \"{code}\" não existe.");
            }
        }
    }

    public async Task<Counterparty> CounterpartyAsync(RequestContext context, string value, CancellationToken cancellationToken)
    {
        var all = await CachedAsync(context, "counterparties", async () =>
            (await counterparties.ListCounterpartiesAsync(new ListCounterpartiesRequest { Context = context }, cancellationToken: cancellationToken))
                .Counterparties.ToList());
        return all.FirstOrDefault(c => Same(c.Code, value) || Same(c.Name, value) || Same(c.Document ?? string.Empty, value))
            ?? throw new LineValueException($"A contraparte \"{value}\" não existe.");
    }

    private static bool Same(string a, string b) => FileTemplates.Normalize(a) == FileTemplates.Normalize(b);

    private async Task<T> CachedAsync<T>(RequestContext context, string name, Func<Task<T>> load) =>
        (await cache.GetOrCreateAsync($"{context.OrganizationId}:{context.UserId}:{name}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = Ttl;
            return load();
        }))!;
}
