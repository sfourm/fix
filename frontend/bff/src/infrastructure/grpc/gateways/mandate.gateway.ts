import type { ApproveMandateCommand } from '../../../application/mandates/commands/approve-mandate.command.js';
import type { CloseMandateCommand } from '../../../application/mandates/commands/close-mandate.command.js';
import type { DeleteMandateCommand } from '../../../application/mandates/commands/delete-mandate.command.js';
import type { IssueMandateCommand } from '../../../application/mandates/commands/issue-mandate.command.js';
import type { RejectMandateCommand } from '../../../application/mandates/commands/reject-mandate.command.js';
import type { UpdateMandateCommand } from '../../../application/mandates/commands/update-mandate.command.js';
import type { ComplianceDto } from '../../../application/mandates/dtos/compliance.dto.js';
import type { MandateDto } from '../../../application/mandates/dtos/mandate.dto.js';
import type { MandateGateway } from '../../../application/mandates/ports/mandate.gateway.js';
import type { GetMandateQuery } from '../../../application/mandates/queries/get-mandate.query.js';
import type { ListMandatesQuery } from '../../../application/mandates/queries/list-mandates.query.js';
import type { PreviewMandateComplianceQuery } from '../../../application/mandates/queries/preview-mandate-compliance.query.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { Page } from '../../../cross-cutting/paging/page.js';
import type { CoreClient } from '../core-client.js';
import { toContractContext, toContractPage, toPage, type ContractPageInfo } from '../mappers/common.contract-mapper.js';
import { mandateStatusEnum, mandateTypeEnum } from '../mappers/enum.contract-mapper.js';
import {
  toComplianceDto,
  toContractMandateTerms,
  toMandateDto,
  type ContractCompliance,
  type ContractMandate,
} from '../mappers/mandate.contract-mapper.js';

const SERVICE = 'MandateService';

export class GrpcMandateGateway implements MandateGateway {
  constructor(private readonly core: CoreClient) {}

  issue({ context, policyId, axisId, type, terms }: IssueMandateCommand): Promise<MandateDto> {
    return this.mandate('IssueMandate', context, {
      policyId,
      axisId,
      type: mandateTypeEnum.toContract(type),
      terms: toContractMandateTerms(terms),
    });
  }

  update({ context, id, terms }: UpdateMandateCommand): Promise<MandateDto> {
    return this.mandate('UpdateMandate', context, { id, terms: toContractMandateTerms(terms) });
  }

  approve({ context, id, note }: ApproveMandateCommand): Promise<MandateDto> {
    return this.mandate('ApproveMandate', context, { id, note });
  }

  reject({ context, id, note }: RejectMandateCommand): Promise<MandateDto> {
    return this.mandate('RejectMandate', context, { id, note });
  }

  close({ context, id, note }: CloseMandateCommand): Promise<MandateDto> {
    return this.mandate('CloseMandate', context, { id, note });
  }

  async delete({ context, id }: DeleteMandateCommand): Promise<void> {
    await this.call('DeleteMandate', context, { id });
  }

  get({ context, id }: GetMandateQuery): Promise<MandateDto> {
    return this.mandate('GetMandate', context, { id });
  }

  async list({ context, policyId, status, page }: ListMandatesQuery): Promise<Page<MandateDto>> {
    const response = await this.call<{ mandates: ContractMandate[]; page: ContractPageInfo | null }>('ListMandates', context, {
      policyId,
      status: mandateStatusEnum.toContract(status),
      page: toContractPage(page),
    });
    return toPage(response.mandates, response.page, toMandateDto);
  }

  async previewCompliance({ context, policyId, axisId, type, terms }: PreviewMandateComplianceQuery): Promise<ComplianceDto> {
    return toComplianceDto(
      await this.call<ContractCompliance>('PreviewMandateCompliance', context, {
        policyId,
        axisId,
        type: mandateTypeEnum.toContract(type),
        terms: toContractMandateTerms(terms),
      }),
    );
  }

  private async mandate(method: string, context: RequestContext, request: object): Promise<MandateDto> {
    return toMandateDto(await this.call<ContractMandate>(method, context, request));
  }

  private call<T>(method: string, context: RequestContext, request: object): Promise<T> {
    return this.core.call<T>(SERVICE, method, { context: toContractContext(context), ...request });
  }
}
