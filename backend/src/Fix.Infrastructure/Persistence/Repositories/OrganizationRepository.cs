using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Organizations.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fix.Infrastructure.Persistence.Repositories;

internal sealed class OrganizationRepository(FixDbContext dbContext) : IOrganizationRepository
{
    public Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Organizations
            .Include(o => o.Members)
            .Include(o => o.Groups).ThenInclude(g => g.Members)
            .Include(o => o.Rules)
            .Include(o => o.Commodities)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Organization>> ListByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.Organizations
            .AsNoTracking()
            .Where(o => o.Members.Any(m => m.UserId == userId))
            .OrderBy(o => o.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> SlugExistsAsync(Slug slug, CancellationToken cancellationToken) =>
        dbContext.Organizations.AnyAsync(o => o.Slug == slug, cancellationToken);

    public Task<bool> IsMemberAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken) =>
        dbContext.OrganizationMembers.AnyAsync(
            m => m.OrganizationId == organizationId && m.UserId == userId,
            cancellationToken);

    public async Task<IReadOnlyCollection<string>> GetRoleCodesAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var memberIds = dbContext.OrganizationMembers
            .Where(m => m.OrganizationId == organizationId && m.UserId == userId)
            .Select(m => m.Id);

        var groupIds = dbContext.OrganizationGroupMembers
            .Where(gm => memberIds.Contains(gm.MemberId))
            .Select(gm => gm.GroupId);

        var ruleIds = dbContext.OrganizationRules
            .Where(r => r.OrganizationId == organizationId
                && ((r.MemberId != null && memberIds.Contains(r.MemberId.Value))
                    || (r.GroupId != null && groupIds.Contains(r.GroupId.Value))))
            .Select(r => r.RuleId);

        var codes = await dbContext.RuleRoles
            .Where(rr => ruleIds.Contains(rr.RuleId))
            .Join(dbContext.Set<Fix.Domain.AggregateRoots.Roles.Role>(), rr => rr.RoleId, r => r.Id, (_, r) => r.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        return codes.ToHashSet();
    }

    public void Add(Organization organization) => dbContext.Organizations.Add(organization);
}

