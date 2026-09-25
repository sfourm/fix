using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Common;
using Fix.Application.Mandates.Commands;
using Fix.Application.Mandates.Dtos;
using Fix.Application.Mandates.Mappers;
using Fix.Application.Mandates.Queries;
using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Mandates.Repositories;
using Fix.Domain.AggregateRoots.Organizations.Repositories;
using Fix.Domain.AggregateRoots.Policies;
using Fix.Domain.AggregateRoots.Policies.Repositories;
using Fix.Domain.AggregateRoots.Roles;
using Fix.Domain.Services;

namespace Fix.Application.Mandates.Services;

internal sealed class MandateService(
    IMandateRepository mandateRepository,
    IPolicyRepository policyRepository,
    IOrganizationRepository organizationRepository,
    IRoleResolver roleResolver,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<IssueMandateCommand, MandateDto>,
      ICommandHandler<UpdateMandateCommand, MandateDto>,
      ICommandHandler<ApproveMandateCommand, MandateDto>,
      ICommandHandler<RejectMandateCommand, MandateDto>,
      ICommandHandler<CloseMandateCommand, MandateDto>,
      ICommandHandler<DeleteMandateCommand, Unit>,
      IQueryHandler<GetMandateQuery, MandateDto>,
      IQueryHandler<ListMandatesQuery, PagedList<MandateDto>>,
      IQueryHandler<PreviewMandateComplianceQuery, ComplianceDto>
{
    // ---------- Commands ----------

    /// <summary>Emite o mandato: dentro da política e com alçada entra ativo; senão vai para a fila de aprovação.</summary>
    public async Task<MandateDto> HandleAsync(IssueMandateCommand command, CancellationToken cancellationToken)
    {
        var policy = await GetPolicyAsync(command.PolicyId, cancellationToken);
        var terms = command.Terms.ToTerms();
        var compliance = await EvaluateAsync(policy, command.OrganizationId, command.Type, terms, cancellationToken);
        var roles = await roleResolver.GetRolesAsync(command.OrganizationId, command.UserId, cancellationToken);

        var mandate = Mandate.Issue(
            policy,
            command.AxisId,
            command.Type,
            terms,
            compliance,
            command.UserId,
            roles.Contains(RoleCodes.SelfApprove));

        mandateRepository.Add(mandate);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mandate.ToDto(policy, consumed: 0);
    }

    public async Task<MandateDto> HandleAsync(UpdateMandateCommand command, CancellationToken cancellationToken)
    {
        var mandate = await GetAsync(command.Id, cancellationToken);
        var policy = await GetPolicyAsync(mandate.PolicyId, cancellationToken);
        var terms = command.Terms.ToTerms();

        mandate.Update(terms, await EvaluateAsync(policy, command.OrganizationId, mandate.Type, terms, cancellationToken));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(mandate, policy, cancellationToken);
    }

    /// <summary>Mandato FORA da política exige, além de approve_mandate, a alçada de exceção.</summary>
    public async Task<MandateDto> HandleAsync(ApproveMandateCommand command, CancellationToken cancellationToken)
    {
        var mandate = await GetAsync(command.Id, cancellationToken);

        if (!mandate.Compliance.IsWithin)
        {
            var roles = await roleResolver.GetRolesAsync(command.OrganizationId, command.UserId, cancellationToken);
            if (!roles.Contains(RoleCodes.ApproveException))
            {
                throw new ForbiddenException(
                    $"Mandato FORA da política — a aprovação exige a role '{RoleCodes.ApproveException}'.");
            }
        }

        mandate.Approve(command.UserId, timeProvider.GetUtcNow(), command.Note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(mandate, null, cancellationToken);
    }

    public async Task<MandateDto> HandleAsync(RejectMandateCommand command, CancellationToken cancellationToken)
    {
        var mandate = await GetAsync(command.Id, cancellationToken);

        mandate.Reject(command.UserId, timeProvider.GetUtcNow(), command.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(mandate, null, cancellationToken);
    }

    public async Task<MandateDto> HandleAsync(CloseMandateCommand command, CancellationToken cancellationToken)
    {
        var mandate = await GetAsync(command.Id, cancellationToken);

        mandate.Close(command.UserId, timeProvider.GetUtcNow(), command.Note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(mandate, null, cancellationToken);
    }

    public async Task<Unit> HandleAsync(DeleteMandateCommand command, CancellationToken cancellationToken)
    {
        var mandate = await GetAsync(command.Id, cancellationToken);
        if (await mandateRepository.HasOrdersAsync(mandate.Id, cancellationToken))
        {
            throw new DomainException("O mandato possui boletas. Encerre o mandato em vez de excluí-lo.");
        }

        mandateRepository.Remove(mandate);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    // ---------- Queries ----------

    public async Task<MandateDto> HandleAsync(GetMandateQuery query, CancellationToken cancellationToken) =>
        await ToDtoAsync(await GetAsync(query.Id, cancellationToken), null, cancellationToken);

    public async Task<PagedList<MandateDto>> HandleAsync(ListMandatesQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        var mandates = await mandateRepository.ListAsync(new MandateFilter(query.PolicyId, query.Status), page, pageSize, cancellationToken);

        var consumed = await mandateRepository.GetConsumedAsync(mandates.Items.Select(m => m.Id).ToList(), null, cancellationToken);
        var policies = new Dictionary<Guid, Policy?>();
        foreach (var policyId in mandates.Items.Select(m => m.PolicyId).Distinct())
        {
            policies[policyId] = await policyRepository.GetByIdAsync(policyId, cancellationToken);
        }

        return mandates.Map(m => m.ToDto(policies[m.PolicyId], consumed.GetValueOrDefault(m.Id)));
    }

    public async Task<ComplianceDto> HandleAsync(PreviewMandateComplianceQuery query, CancellationToken cancellationToken)
    {
        var policy = await GetPolicyAsync(query.PolicyId, cancellationToken);
        var axis = policy.GetAxis(query.AxisId);
        if (axis.Factor != query.Type.Factor())
        {
            return Compliance.Outside($"o eixo {axis.Code} não cobre este tipo de mandato").ToDto();
        }

        return (await EvaluateAsync(policy, query.OrganizationId, query.Type, query.Terms.ToTerms(), cancellationToken)).ToDto();
    }

    // ---------- Helpers ----------

    private async Task<Compliance> EvaluateAsync(
        Policy policy,
        Guid organizationId,
        MandateType type,
        MandateTerms terms,
        CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(organizationId, cancellationToken)
            ?? throw new NotFoundException("Organização", organizationId);

        return MandateCompliance.Evaluate(policy, organization.Budget, type, terms, timeProvider.Today());
    }

    private async Task<MandateDto> ToDtoAsync(Mandate mandate, Policy? policy, CancellationToken cancellationToken)
    {
        policy ??= await policyRepository.GetByIdAsync(mandate.PolicyId, cancellationToken);
        var consumed = await mandateRepository.GetConsumedAsync([mandate.Id], null, cancellationToken);
        return mandate.ToDto(policy, consumed.GetValueOrDefault(mandate.Id));
    }

    private async Task<Mandate> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await mandateRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Mandato", id);

    private async Task<Policy> GetPolicyAsync(Guid id, CancellationToken cancellationToken) =>
        await policyRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Política", id);
}

