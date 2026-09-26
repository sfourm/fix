import type { ApprovalStatus } from '../../../cross-cutting/enums/approval-status.js';
import type { Commodity } from '../../../cross-cutting/enums/commodity.js';
import type { ComplianceStatus } from '../../../cross-cutting/enums/compliance-status.js';
import type { ConfirmationStatus } from '../../../cross-cutting/enums/confirmation-status.js';
import type { CounterpartyType } from '../../../cross-cutting/enums/counterparty-type.js';
import type { Desk } from '../../../cross-cutting/enums/desk.js';
import type { FileKind } from '../../../cross-cutting/enums/file-kind.js';
import type { FileLineStatus } from '../../../cross-cutting/enums/file-line-status.js';
import type { FileStatus } from '../../../cross-cutting/enums/file-status.js';
import type { InstrumentPermission } from '../../../cross-cutting/enums/instrument-permission.js';
import type { MandateStatus } from '../../../cross-cutting/enums/mandate-status.js';
import type { MandateType } from '../../../cross-cutting/enums/mandate-type.js';
import type { MeasurementUnit } from '../../../cross-cutting/enums/measurement-unit.js';
import type { OptionKind } from '../../../cross-cutting/enums/option-kind.js';
import type { OrderType } from '../../../cross-cutting/enums/order-type.js';
import type { PolicyStatus } from '../../../cross-cutting/enums/policy-status.js';
import type { RiskFactor } from '../../../cross-cutting/enums/risk-factor.js';
import type { Sector } from '../../../cross-cutting/enums/sector.js';
import type { TradeDirection } from '../../../cross-cutting/enums/trade-direction.js';

/**
 * Converte os enums do contrato (COMMODITY_RAW_SUGAR, com _UNSPECIFIED = 0) para os valores
 * PascalCase usados no BFF e no web (RawSugar), e vice-versa.
 */
export interface ContractEnum<T extends string> {
  toContract(value: T | null | undefined): string;
  fromContract(value: string | null | undefined): T | null;
  fromContractRequired(value: string | null | undefined): T;
}

export function contractEnum<T extends string>(prefix: string): ContractEnum<T> {
  const unspecified = `${prefix}_UNSPECIFIED`;

  const fromContract = (value: string | null | undefined): T | null => {
    if (!value || value === unspecified || !value.startsWith(`${prefix}_`)) {
      return null;
    }

    return value
      .slice(prefix.length + 1)
      .split('_')
      .map((word) => word.charAt(0) + word.slice(1).toLowerCase())
      .join('') as T;
  };

  return {
    toContract: (value) =>
      value ? `${prefix}_${value.replace(/([a-z0-9])([A-Z])/g, '$1_$2').toUpperCase()}` : unspecified,
    fromContract,
    fromContractRequired: (value) => {
      const result = fromContract(value);
      if (result === null) {
        throw new Error(`Valor de enum ${prefix} inválido no contrato: ${value}`);
      }

      return result;
    },
  };
}

export const commodityEnum = contractEnum<Commodity>('COMMODITY');
export const measurementUnitEnum = contractEnum<MeasurementUnit>('MEASUREMENT_UNIT');
export const sectorEnum = contractEnum<Sector>('SECTOR');
export const deskEnum = contractEnum<Desk>('DESK');
export const counterpartyTypeEnum = contractEnum<CounterpartyType>('COUNTERPARTY_TYPE');
export const policyStatusEnum = contractEnum<PolicyStatus>('POLICY_STATUS');
export const riskFactorEnum = contractEnum<RiskFactor>('RISK_FACTOR');
export const instrumentPermissionEnum = contractEnum<InstrumentPermission>('INSTRUMENT_PERMISSION');
export const mandateTypeEnum = contractEnum<MandateType>('MANDATE_TYPE');
export const mandateStatusEnum = contractEnum<MandateStatus>('MANDATE_STATUS');
export const complianceStatusEnum = contractEnum<ComplianceStatus>('COMPLIANCE_STATUS');
export const orderTypeEnum = contractEnum<OrderType>('ORDER_TYPE');
export const tradeDirectionEnum = contractEnum<TradeDirection>('TRADE_DIRECTION');
export const optionKindEnum = contractEnum<OptionKind>('OPTION_KIND');
export const approvalStatusEnum = contractEnum<ApprovalStatus>('APPROVAL_STATUS');
export const confirmationStatusEnum = contractEnum<ConfirmationStatus>('CONFIRMATION_STATUS');
export const fileKindEnum = contractEnum<FileKind>('FILE_KIND');
export const fileStatusEnum = contractEnum<FileStatus>('FILE_STATUS');
export const fileLineStatusEnum = contractEnum<FileLineStatus>('FILE_LINE_STATUS');
