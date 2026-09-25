using Fix.Application.Abstractions.Exceptions;
using Fix.Domain.AggregateRoots.Organizations.Repositories;

namespace Fix.Application.Authorization;

/// <summary>
/// Alçada pelo organograma: quem decide (aprova ou rejeita) um pedido precisa estar num grupo acima de quem
/// o fez. Complementa a role (approve_mandate, approve_order): ter a role não basta se a pessoa não está acima.
/// A equipe interna FIX não decide pelas organizações (nem tem as roles de decisão).
/// </summary>
internal sealed class OrgChartApproval(IOrganizationRepository organizationRepository)
{
    public async Task EnsureCanDecideAsync(Guid organizationId, Guid approverUserId, Guid requesterUserId, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new NotFoundException("Organização", organizationId);

        if (!organization.IsAboveInOrgChart(approverUserId, requesterUserId))
        {
            throw new ForbiddenException(approverUserId == requesterUserId
                ? "Ninguém decide o próprio pedido: a decisão cabe a quem está acima no organograma."
                : "Só quem está num grupo acima de quem fez o pedido, no organograma, pode decidir.");
        }
    }
}
