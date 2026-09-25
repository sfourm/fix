import { mapPage, type Page } from '../../../cross-cutting/paging/page.js';
import type { AddCoverageBandCommand } from '../commands/add-coverage-band.command.js';
import type { AddPolicyAxisCommand } from '../commands/add-policy-axis.command.js';
import type { AddPolicyInstrumentCommand } from '../commands/add-policy-instrument.command.js';
import type { ApprovePolicyCommand } from '../commands/approve-policy.command.js';
import type { CreatePolicyCommand } from '../commands/create-policy.command.js';
import type { DeletePolicyCommand } from '../commands/delete-policy.command.js';
import type { OpenPolicyVersionCommand } from '../commands/open-policy-version.command.js';
import type { RemoveCoverageBandCommand } from '../commands/remove-coverage-band.command.js';
import type { RemovePolicyAxisCommand } from '../commands/remove-policy-axis.command.js';
import type { RemovePolicyInstrumentCommand } from '../commands/remove-policy-instrument.command.js';
import type { SubmitPolicyCommand } from '../commands/submit-policy.command.js';
import type { UpdateCoverageBandCommand } from '../commands/update-coverage-band.command.js';
import type { UpdatePolicyAxisCommand } from '../commands/update-policy-axis.command.js';
import type { UpdatePolicyInstrumentCommand } from '../commands/update-policy-instrument.command.js';
import type { UpdatePolicyLimitsCommand } from '../commands/update-policy-limits.command.js';
import type { UpdatePolicyCommand } from '../commands/update-policy.command.js';
import type { PolicyDto } from '../dtos/policy.dto.js';
import { toPolicyResponse, toPolicySummaryResponse } from '../mappers/policy.mapper.js';
import type { PolicyGateway } from '../ports/policy.gateway.js';
import type { GetPolicyQuery } from '../queries/get-policy.query.js';
import type { ListPoliciesQuery } from '../queries/list-policies.query.js';
import type { PolicySummaryResponse } from '../responses/policy-summary.response.js';
import type { PolicyResponse } from '../responses/policy.response.js';

/** Política de riscos versionada: ciclo de aprovação, parâmetros, eixos, bandas e instrumentos. */
export class PolicyService {
  constructor(private readonly gateway: PolicyGateway) {}

  create = (command: CreatePolicyCommand) => this.respond(this.gateway.create(command));
  update = (command: UpdatePolicyCommand) => this.respond(this.gateway.update(command));
  updateLimits = (command: UpdatePolicyLimitsCommand) => this.respond(this.gateway.updateLimits(command));
  submit = (command: SubmitPolicyCommand) => this.respond(this.gateway.submit(command));
  approve = (command: ApprovePolicyCommand) => this.respond(this.gateway.approve(command));
  openVersion = (command: OpenPolicyVersionCommand) => this.respond(this.gateway.openVersion(command));

  addAxis = (command: AddPolicyAxisCommand) => this.respond(this.gateway.addAxis(command));
  updateAxis = (command: UpdatePolicyAxisCommand) => this.respond(this.gateway.updateAxis(command));
  removeAxis = (command: RemovePolicyAxisCommand) => this.respond(this.gateway.removeAxis(command));
  addBand = (command: AddCoverageBandCommand) => this.respond(this.gateway.addBand(command));
  updateBand = (command: UpdateCoverageBandCommand) => this.respond(this.gateway.updateBand(command));
  removeBand = (command: RemoveCoverageBandCommand) => this.respond(this.gateway.removeBand(command));
  addInstrument = (command: AddPolicyInstrumentCommand) => this.respond(this.gateway.addInstrument(command));
  updateInstrument = (command: UpdatePolicyInstrumentCommand) => this.respond(this.gateway.updateInstrument(command));
  removeInstrument = (command: RemovePolicyInstrumentCommand) => this.respond(this.gateway.removeInstrument(command));

  get = (query: GetPolicyQuery) => this.respond(this.gateway.get(query));

  async delete(command: DeletePolicyCommand): Promise<void> {
    await this.gateway.delete(command);
  }

  async list(query: ListPoliciesQuery): Promise<Page<PolicySummaryResponse>> {
    return mapPage(await this.gateway.list(query), toPolicySummaryResponse);
  }

  private async respond(policy: Promise<PolicyDto>): Promise<PolicyResponse> {
    return toPolicyResponse(await policy);
  }
}
