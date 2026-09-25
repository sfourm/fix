using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Common;
using Fix.Application.Policies.Commands;
using Fix.Application.Policies.Dtos;
using Fix.Application.Policies.Mappers;
using Fix.Application.Policies.Queries;
using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Organizations.Repositories;
using Fix.Domain.AggregateRoots.Policies;
using Fix.Domain.AggregateRoots.Policies.Repositories;

namespace Fix.Application.Policies.Services;

internal sealed class PolicyService(
    IPolicyRepository policyRepository,
    IOrganizationRepository organizationRepository,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePolicyCommand, PolicyDto>,
      ICommandHandler<UpdatePolicyCommand, PolicyDto>,
      ICommandHandler<UpdatePolicyLimitsCommand, PolicyDto>,
      ICommandHandler<SubmitPolicyCommand, PolicyDto>,
      ICommandHandler<ApprovePolicyCommand, PolicyDto>,
      ICommandHandler<OpenPolicyVersionCommand, PolicyDto>,
      ICommandHandler<DeletePolicyCommand, Unit>,
      ICommandHandler<AddPolicyAxisCommand, PolicyDto>,
      ICommandHandler<UpdatePolicyAxisCommand, PolicyDto>,
      ICommandHandler<RemovePolicyAxisCommand, PolicyDto>,
      ICommandHandler<AddCoverageBandCommand, PolicyDto>,
      ICommandHandler<UpdateCoverageBandCommand, PolicyDto>,
      ICommandHandler<RemoveCoverageBandCommand, PolicyDto>,
      ICommandHandler<AddPolicyInstrumentCommand, PolicyDto>,
      ICommandHandler<UpdatePolicyInstrumentCommand, PolicyDto>,
      ICommandHandler<RemovePolicyInstrumentCommand, PolicyDto>,
      IQueryHandler<GetPolicyQuery, PolicyDto>,
      IQueryHandler<ListPoliciesQuery, PagedList<PolicySummaryDto>>
{
    // ---------- Commands: política ----------

    public async Task<PolicyDto> HandleAsync(CreatePolicyCommand command, CancellationToken cancellationToken)
    {
        await EnsureUniqueCodeAsync(command.Code, null, cancellationToken);

        var policy = Policy.Create(
            command.OrganizationId,
            command.Code,
            Title.Create(command.Title),
            command.Version,
            command.Description,
            DateRange.Create(command.ValidFrom, command.ValidTo),
            timeProvider.Today());

        if (command.UseTemplate)
        {
            var organization = await organizationRepository.GetByIdAsync(command.OrganizationId, cancellationToken)
                ?? throw new NotFoundException("Organização", command.OrganizationId);
            var activeCrop = organization.Profile.ActiveCrop is { } crop ? CropYear.Create(crop) : null;
            PolicyTemplate.Apply(policy, activeCrop);
        }

        policyRepository.Add(policy);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return policy.ToDto();
    }

    public async Task<PolicyDto> HandleAsync(UpdatePolicyCommand command, CancellationToken cancellationToken)
    {
        await EnsureUniqueCodeAsync(command.Code, command.Id, cancellationToken);

        return await ChangeAsync(command.Id, p => p.UpdateGeneral(
            command.Code,
            Title.Create(command.Title),
            command.Description,
            DateRange.Create(command.ValidFrom, command.ValidTo)), cancellationToken);
    }

    public Task<PolicyDto> HandleAsync(UpdatePolicyLimitsCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, p => p.UpdateLimits(command.Limits.ToDomain()), cancellationToken);

    public Task<PolicyDto> HandleAsync(SubmitPolicyCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, p => p.Submit(timeProvider.Today()), cancellationToken);

    /// <summary>Aprova com ata; a política vigente anterior da companhia passa a "substituída".</summary>
    public async Task<PolicyDto> HandleAsync(ApprovePolicyCommand command, CancellationToken cancellationToken)
    {
        var policy = await GetAsync(command.Id, cancellationToken);
        var today = timeProvider.Today();

        foreach (var active in (await policyRepository.ListActiveAsync(cancellationToken)).Where(p => p.Id != policy.Id))
        {
            active.Supersede(today);
        }

        policy.Approve(command.ApprovalRecord, today);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return policy.ToDto();
    }

    public Task<PolicyDto> HandleAsync(OpenPolicyVersionCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, p => p.OpenNewVersion(command.Version, command.Reason, timeProvider.Today()), cancellationToken);

    public async Task<Unit> HandleAsync(DeletePolicyCommand command, CancellationToken cancellationToken)
    {
        var policy = await GetAsync(command.Id, cancellationToken);

        if (policy.Status != PolicyStatus.Draft)
        {
            throw new DomainException("Somente políticas em rascunho podem ser excluídas.");
        }

        if (await policyRepository.HasMandatesAsync(policy.Id, cancellationToken))
        {
            throw new DomainException("A política possui mandatos e não pode ser excluída.");
        }

        policyRepository.Remove(policy);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    // ---------- Commands: eixos, bandas e instrumentos ----------

    public Task<PolicyDto> HandleAsync(AddPolicyAxisCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.PolicyId, p => p.AddAxis(
            command.Axis.Code,
            Title.Create(command.Axis.Title),
            command.Axis.Factor,
            command.Axis.Statement,
            command.Axis.LimitDescription,
            command.Axis.Approver,
            command.Axis.Restrictions), cancellationToken);

    public Task<PolicyDto> HandleAsync(UpdatePolicyAxisCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.PolicyId, p => p.UpdateAxis(
            command.AxisId,
            command.Axis.Code,
            Title.Create(command.Axis.Title),
            command.Axis.Factor,
            command.Axis.Statement,
            command.Axis.LimitDescription,
            command.Axis.Approver,
            command.Axis.Restrictions), cancellationToken);

    public async Task<PolicyDto> HandleAsync(RemovePolicyAxisCommand command, CancellationToken cancellationToken)
    {
        if (await policyRepository.AxisHasMandatesAsync(command.AxisId, cancellationToken))
        {
            throw new DomainException("O eixo possui mandatos vinculados e não pode ser removido.");
        }

        return await ChangeAsync(command.PolicyId, p => p.RemoveAxis(command.AxisId), cancellationToken);
    }

    public Task<PolicyDto> HandleAsync(AddCoverageBandCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.PolicyId, p => p.AddBand(
            command.Band.Horizon,
            CropYear.Create(command.Band.Crop),
            command.Band.MinPct,
            command.Band.MaxPct,
            command.Band.Note), cancellationToken);

    public Task<PolicyDto> HandleAsync(UpdateCoverageBandCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.PolicyId, p => p.UpdateBand(
            command.BandId,
            command.Band.Horizon,
            CropYear.Create(command.Band.Crop),
            command.Band.MinPct,
            command.Band.MaxPct,
            command.Band.Note), cancellationToken);

    public Task<PolicyDto> HandleAsync(RemoveCoverageBandCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.PolicyId, p => p.RemoveBand(command.BandId), cancellationToken);

    public Task<PolicyDto> HandleAsync(AddPolicyInstrumentCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.PolicyId, p => p.AddInstrument(
            command.Instrument.Name,
            command.Instrument.Permission,
            command.Instrument.Condition), cancellationToken);

    public Task<PolicyDto> HandleAsync(UpdatePolicyInstrumentCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.PolicyId, p => p.UpdateInstrument(
            command.InstrumentId,
            command.Instrument.Name,
            command.Instrument.Permission,
            command.Instrument.Condition), cancellationToken);

    public Task<PolicyDto> HandleAsync(RemovePolicyInstrumentCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.PolicyId, p => p.RemoveInstrument(command.InstrumentId), cancellationToken);

    // ---------- Queries ----------

    public async Task<PolicyDto> HandleAsync(GetPolicyQuery query, CancellationToken cancellationToken) =>
        (await GetAsync(query.Id, cancellationToken)).ToDto();

    public async Task<PagedList<PolicySummaryDto>> HandleAsync(ListPoliciesQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        var policies = await policyRepository.ListAsync(page, pageSize, cancellationToken);
        return policies.Map(p => p.ToSummaryDto());
    }

    // ---------- Helpers ----------

    private async Task<PolicyDto> ChangeAsync(Guid id, Action<Policy> change, CancellationToken cancellationToken)
    {
        var policy = await GetAsync(id, cancellationToken);

        change(policy);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return policy.ToDto();
    }

    // O query filter de tenant do EF garante que só políticas da organização atual são encontradas.
    private async Task<Policy> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await policyRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Política", id);

    private async Task EnsureUniqueCodeAsync(string code, Guid? exceptId, CancellationToken cancellationToken)
    {
        if (await policyRepository.CodeExistsAsync(code.Trim().ToUpperInvariant(), exceptId, cancellationToken))
        {
            throw new ConflictException($"Já existe uma política com o código '{code.Trim().ToUpperInvariant()}'.");
        }
    }
}

