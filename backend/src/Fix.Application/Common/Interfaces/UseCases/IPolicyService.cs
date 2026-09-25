using Fix.Application.Policies.Commands;
using Fix.Application.Policies.Dtos;
using Fix.Application.Policies.Queries;
using Fix.Domain.Abstractions;

namespace Fix.Application.Common.Interfaces.UseCases;

/// <summary>Política de riscos versionada: ciclo de aprovação, parâmetros, eixos, bandas e instrumentos. A entrada é validada na presentation (IValidationFactory) e autorizada pelo UseCaseGuard.</summary>
public interface IPolicyService
{
    Task<PolicyDto> CreatePolicyAsync(CreatePolicyCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> UpdatePolicyAsync(UpdatePolicyCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> UpdatePolicyLimitsAsync(UpdatePolicyLimitsCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> SubmitPolicyAsync(SubmitPolicyCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> ApprovePolicyAsync(ApprovePolicyCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> OpenPolicyVersionAsync(OpenPolicyVersionCommand command, CancellationToken cancellationToken);

    Task DeletePolicyAsync(DeletePolicyCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> AddPolicyAxisAsync(AddPolicyAxisCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> UpdatePolicyAxisAsync(UpdatePolicyAxisCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> RemovePolicyAxisAsync(RemovePolicyAxisCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> AddCoverageBandAsync(AddCoverageBandCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> UpdateCoverageBandAsync(UpdateCoverageBandCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> RemoveCoverageBandAsync(RemoveCoverageBandCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> AddPolicyInstrumentAsync(AddPolicyInstrumentCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> UpdatePolicyInstrumentAsync(UpdatePolicyInstrumentCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> RemovePolicyInstrumentAsync(RemovePolicyInstrumentCommand command, CancellationToken cancellationToken);

    Task<PolicyDto> GetPolicyAsync(GetPolicyQuery query, CancellationToken cancellationToken);

    Task<PagedList<PolicySummaryDto>> ListPoliciesAsync(ListPoliciesQuery query, CancellationToken cancellationToken);
}
