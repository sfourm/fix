import type { FieldOption } from './field-definition.js';

const options = (labels: Record<string, string>): FieldOption[] =>
  Object.entries(labels).map(([value, label]) => ({ value, label }));

/** Rótulos pt-BR dos valores de enum, usados nos filtros e nas categorias dos gráficos. */
export const optionLabels = {
  commodity: options({
    RawSugar: 'Açúcar VHP',
    WhiteSugar: 'Açúcar branco',
    HydratedEthanol: 'Etanol hidratado',
    AnhydrousEthanol: 'Etanol anidro',
    Corn: 'Milho',
    Soybean: 'Soja',
  }),
  approval: options({ PendingApproval: 'Pendente', Approved: 'Aprovada', Rejected: 'Rejeitada' }),
  confirmation: options({ Pending: 'Pendente', Confirmed: 'Confirmado', Divergent: 'Divergente', Refused: 'Recusado' }),
  orderType: options({ Futures: 'Futuro', Option: 'Opção', Ndf: 'NDF' }),
  direction: options({ Buy: 'Compra', Sell: 'Venda' }),
  optionKind: options({ Call: 'Call', Put: 'Put' }),
  mandateStatus: options({ PendingApproval: 'Pendente', Active: 'Ativo', Rejected: 'Rejeitado', Closed: 'Encerrado' }),
  mandateType: options({ Pricing: 'Precificação', Currency: 'Moeda', Commercial: 'Comercial', Logistics: 'Logística' }),
  compliance: options({ Within: 'Dentro da política', Outside: 'Fora da política' }),
  counterpartyType: options({
    Trading: 'Trading',
    BankTrading: 'Banco / trading',
    OtcCounterparty: 'Contraparte OTC',
    Broker: 'Corretora',
    Carrier: 'Transportadora',
    Producer: 'Produtor',
  }),
  policyStatus: options({ Draft: 'Rascunho', UnderApproval: 'Em aprovação', Active: 'Vigente', Superseded: 'Substituída' }),
};
