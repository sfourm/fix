import type { AddCoverageBandCommand } from '../../../application/policies/commands/add-coverage-band.command.js';
import type { AddPolicyAxisCommand } from '../../../application/policies/commands/add-policy-axis.command.js';
import type { AddPolicyInstrumentCommand } from '../../../application/policies/commands/add-policy-instrument.command.js';
import type { ApprovePolicyCommand } from '../../../application/policies/commands/approve-policy.command.js';
import type { CreatePolicyCommand } from '../../../application/policies/commands/create-policy.command.js';
import type { DeletePolicyCommand } from '../../../application/policies/commands/delete-policy.command.js';
import type { OpenPolicyVersionCommand } from '../../../application/policies/commands/open-policy-version.command.js';
import type { RemoveCoverageBandCommand } from '../../../application/policies/commands/remove-coverage-band.command.js';
import type { RemovePolicyAxisCommand } from '../../../application/policies/commands/remove-policy-axis.command.js';
import type { RemovePolicyInstrumentCommand } from '../../../application/policies/commands/remove-policy-instrument.command.js';
import type { SubmitPolicyCommand } from '../../../application/policies/commands/submit-policy.command.js';
import type { UpdateCoverageBandCommand } from '../../../application/policies/commands/update-coverage-band.command.js';
import type { UpdatePolicyAxisCommand } from '../../../application/policies/commands/update-policy-axis.command.js';
import type { UpdatePolicyInstrumentCommand } from '../../../application/policies/commands/update-policy-instrument.command.js';
import type { UpdatePolicyLimitsCommand } from '../../../application/policies/commands/update-policy-limits.command.js';
import type { UpdatePolicyCommand } from '../../../application/policies/commands/update-policy.command.js';
import type { PolicySummaryDto } from '../../../application/policies/dtos/policy-summary.dto.js';
import type { PolicyDto } from '../../../application/policies/dtos/policy.dto.js';
import type { PolicyGateway } from '../../../application/policies/ports/policy.gateway.js';
import type { GetPolicyQuery } from '../../../application/policies/queries/get-policy.query.js';
import type { ListPoliciesQuery } from '../../../application/policies/queries/list-policies.query.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { Page } from '../../../cross-cutting/paging/page.js';
import type { CoreClient } from '../core-client.js';
import { toContractContext, toContractPage, toPage, type ContractPageInfo } from '../mappers/common.contract-mapper.js';
import {
  toContractAxisInput,
  toContractBandInput,
  toContractInstrumentInput,
  toPolicyDto,
  toPolicySummaryDto,
  type ContractPolicy,
  type ContractPolicySummary,
} from '../mappers/policy.contract-mapper.js';

const SERVICE = 'PolicyService';

export class GrpcPolicyGateway implements PolicyGateway {
  constructor(private readonly core: CoreClient) {}

  create({ context, ...data }: CreatePolicyCommand): Promise<PolicyDto> {
    return this.policy('CreatePolicy', context, data);
  }

  update({ context, ...data }: UpdatePolicyCommand): Promise<PolicyDto> {
    return this.policy('UpdatePolicy', context, data);
  }

  updateLimits({ context, id, limits }: UpdatePolicyLimitsCommand): Promise<PolicyDto> {
    return this.policy('UpdatePolicyLimits', context, { id, limits });
  }

  submit({ context, id }: SubmitPolicyCommand): Promise<PolicyDto> {
    return this.policy('SubmitPolicy', context, { id });
  }

  approve({ context, id, approvalRecord }: ApprovePolicyCommand): Promise<PolicyDto> {
    return this.policy('ApprovePolicy', context, { id, approvalRecord });
  }

  openVersion({ context, ...data }: OpenPolicyVersionCommand): Promise<PolicyDto> {
    return this.policy('OpenPolicyVersion', context, data);
  }

  async delete({ context, id }: DeletePolicyCommand): Promise<void> {
    await this.call('DeletePolicy', context, { id });
  }

  addAxis({ context, policyId, axis }: AddPolicyAxisCommand): Promise<PolicyDto> {
    return this.policy('AddPolicyAxis', context, { policyId, axis: toContractAxisInput(axis) });
  }

  updateAxis({ context, policyId, axisId, axis }: UpdatePolicyAxisCommand): Promise<PolicyDto> {
    return this.policy('UpdatePolicyAxis', context, { policyId, axisId, axis: toContractAxisInput(axis) });
  }

  removeAxis({ context, policyId, axisId }: RemovePolicyAxisCommand): Promise<PolicyDto> {
    return this.policy('RemovePolicyAxis', context, { policyId, childId: axisId });
  }

  addBand({ context, policyId, band }: AddCoverageBandCommand): Promise<PolicyDto> {
    return this.policy('AddCoverageBand', context, { policyId, band: toContractBandInput(band) });
  }

  updateBand({ context, policyId, bandId, band }: UpdateCoverageBandCommand): Promise<PolicyDto> {
    return this.policy('UpdateCoverageBand', context, { policyId, bandId, band: toContractBandInput(band) });
  }

  removeBand({ context, policyId, bandId }: RemoveCoverageBandCommand): Promise<PolicyDto> {
    return this.policy('RemoveCoverageBand', context, { policyId, childId: bandId });
  }

  addInstrument({ context, policyId, instrument }: AddPolicyInstrumentCommand): Promise<PolicyDto> {
    return this.policy('AddPolicyInstrument', context, { policyId, instrument: toContractInstrumentInput(instrument) });
  }

  updateInstrument({ context, policyId, instrumentId, instrument }: UpdatePolicyInstrumentCommand): Promise<PolicyDto> {
    return this.policy('UpdatePolicyInstrument', context, {
      policyId,
      instrumentId,
      instrument: toContractInstrumentInput(instrument),
    });
  }

  removeInstrument({ context, policyId, instrumentId }: RemovePolicyInstrumentCommand): Promise<PolicyDto> {
    return this.policy('RemovePolicyInstrument', context, { policyId, childId: instrumentId });
  }

  get({ context, id }: GetPolicyQuery): Promise<PolicyDto> {
    return this.policy('GetPolicy', context, { id });
  }

  async list({ context, page }: ListPoliciesQuery): Promise<Page<PolicySummaryDto>> {
    const response = await this.call<{ policies: ContractPolicySummary[]; page: ContractPageInfo | null }>(
      'ListPolicies',
      context,
      { page: toContractPage(page) },
    );
    return toPage(response.policies, response.page, toPolicySummaryDto);
  }

  private async policy(method: string, context: RequestContext, request: object): Promise<PolicyDto> {
    return toPolicyDto(await this.call<ContractPolicy>(method, context, request));
  }

  private call<T>(method: string, context: RequestContext, request: object): Promise<T> {
    return this.core.call<T>(SERVICE, method, { context: toContractContext(context), ...request });
  }
}
