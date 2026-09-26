# Conformidade com o FIX2 (onboarding v60)

Fonte de verdade do negócio: [`example/FIX2_v60_onboarding_dev.pdf`](../example/FIX2_v60_onboarding_dev.pdf) (onboarding
técnico, base FIX2 v60, 24/09/2026) e o protótipo [`example/FIX2_v60.html`](../example/FIX2_v60.html). Este arquivo compara o
que o Fix implementa com o que o documento define. **Ao implementar algo desta lista, atualize o status aqui no mesmo PR.**

Legenda: ✅ atende · ⚠️ diverge (existe, mas com regra diferente) · ❌ não existe ainda.

## 1. Núcleo: Política → Mandato → Boleta

| Item do PDF | Status | Como está no Fix |
| --- | --- | --- |
| Cada camada só **estreita** a anterior | ✅ | Mandato sobre eixo da política (tipo compatível com o fator); boleta sobre mandato ativo e dentro do saldo. |
| Política versionada; **sem ata não vige** | ✅ | `Draft → UnderApproval → Active → Superseded`; aprovar exige ata; mudança em vigente = nova versão. |
| 5 eixos (físico, preço, moeda, frete, entregas) | ✅ | Modelo FIX cria 5 eixos; "entregas" usa o fator Físico (4 fatores: físico, preço, moeda, frete). |
| Mandato classificado **automaticamente** dentro/FORA na emissão; FORA nasce pendente | ✅ | `MandateCompliance`: vigência, horizonte, janela, piso econômico e gatilho de preço. |
| Classificação cobre teto `banda_máx × disponível`, banda FX e tarifa 105% (I-03) | ⚠️ | Faltam o teto por disponível (não há produção/disponível), a banda FX e a tarifa de frete. |
| **FORA aprovado continua FORA** (carimbo + aprovador) | ✅ | O enquadramento e quem decidiu ficam gravados no mandato. |
| Mandato: volume em lotes **ou** notional US$, target/mín/máx ou a mercado, janela | ✅ | `MandateTerms` + `PriceCriteria`. |
| Mandato: **cadências** (tempo/preço, combo e/ou) e saldo **carry/expira** | ❌ | Só há o critério em texto livre. Decisão provisória no PDF: carry por padrão. |
| Boleta: fixação (futuro), opção (put/call, prêmio) e NDF (nocional, taxa) | ✅ | `OrderType` Futures/Option/Ndf; venda de opção marca se é coberta. Faltam campos da NDF (fixing, datas de fixing/liquidação, spot) e da opção (exercício, vencimento). |
| **Pendente não consome** nada (I-02) | ✅ | `ConsumedQuantity` só conta aprovadas; dashboards filtram só válidos (ver §4). |
| Rejeição exige justificativa; tudo logado | ✅ | Mandato e boleta; auditoria em `timelines`. |
| Confirmation em 2 d.u.; régua conforme > pendente > atrasado > divergente > recusado | ✅ | `Pending/Confirmed/Divergent/Refused` + atraso após 2 d.u.; divergente/recusado continuam no risco e ficam "em aberto" até conciliar. |

## 2. "Desvio não bloqueia — expõe"

| Item do PDF | Status | Como está no Fix |
| --- | --- | --- |
| Boleta **sem mandato** é possível, mas sempre sinalizada (I-01) | ✅ | Registrada com justificativa obrigatória, FORA, na fila de aprovação, com carimbo "sem mandato" e listada em **Exceções**. |
| **Vínculo a posteriori** carimbado para sempre | ✅ | `LinkOrderMandate` com justificativa: carimbo `linkedAfterExecution` permanente e auditado. |
| Estourar saldo / descasar tela e vencimento: avisa, permite com justificativa | ✅ | Estouro do saldo do mandato e tela diferente da do mandato: prévia no formulário, justificativa, FORA e carimbo. Na aprovação o saldo é reconferido. Cronograma de contrato ainda não existe (§3). |

## 3. Cadeia física e mercado

| Item do PDF | Status | Como está no Fix |
| --- | --- | --- |
| Produção mensal por safra (manual/CSV/ERP, realizado × projeção) | ❌ | Só capacidade industrial no setup. |
| **Disponível** = produção − contingência escalonada (I-04) | ❌ | Sem produção, não há denominador; percentuais de cobertura não são calculados. |
| Contrato comercial (CTR-), cronograma de precificação, tranches (TR-) | ❌ | Mandatos comerciais existem, mas "são executados por contratos" que ainda não existem. |
| Frete (FRT-): transportadora, tarifa × referência, teto 105% | ❌ | Só o parâmetro na política. |
| Mercado (NY11, câmbio, FG/A) e **MTM** (fixação, NDF, opção, frete) | ❌ | Sem preços de mercado; limites de MtM de contraparte são só cadastro. |

## 4. Invariantes (I-01..I-10)

| # | Status | Observação |
| --- | --- | --- |
| I-01 Boleta sem mandato sinalizada | ✅ | Ver §2. |
| I-02 Pendente fora de todo agregado | ✅ | Consumo de saldo e **widgets de dashboard** (boleta só aprovada; mandato só ativo/encerrado, salvo filtro explícito). |
| I-03 Classificação calculada, nunca digitada | ⚠️ | Calculada; faltam critérios (ver §1). |
| I-04 Contingência fora da base; revisão de guidance recalcula | ❌ | Depende da produção/disponível. |
| I-05 Toda mutação na trilha | ✅ | Interceptor de auditoria (quem, o quê, quando, antes/depois); tela e aba de Auditoria. |
| I-06 Segregação (emitir ≠ aprovar; middle office não executa; último aprovador não sai) | ✅ | Ninguém decide o próprio pedido (organograma); quem executou a boleta não registra o confirmation dela; o owner (acesso total, na raiz do organograma) não pode ser removido, então sempre há aprovador. |
| I-07 Todo % declara a base | ⚠️ | Vale para os % existentes; cobertura por disponível ainda não existe. |
| I-08 Vedados e venda coberta > 15% são FORA | ⚠️ | Venda **descoberta** de opção é FORA por definição (justificativa + aprovação). O teto de 15% da venda coberta depende do disponível (§3). |
| I-09 Selo "est." em valor simulado | ✅ | Não há valores simulados (o Fix não simula mercado). |
| I-10 pt-BR, dd/mm/aaaa, telas ICE, ids por prefixo | ✅ | MD-01 (mandato), HX-0001 (boleta) e CP-01 (contraparte), sequenciais por organização; o id técnico segue UUID. CTR-/TR-/FRT- chegam com contratos e frete (§3). |

## 5. Motor de enquadramento e visão executiva

| Item do PDF | Status | Como está no Fix |
| --- | --- | --- |
| `polEnq`: 8 fatores (cobertura, preço, câmbio, caixa, contrapartes, logística, mix, controles) | ❌ | Não há painel de fatores. |
| `bolEnq`: chip de enquadramento por boleta, critério a critério | ⚠️ | Enquadramento dentro/FORA com motivo e carimbos (sem mandato, a posteriori, estouro, tela, venda descoberta) em tabelas e no detalhe. Faltam critérios que dependem de disponível, mercado e contraparte (cobertura, preço vs piso, limites). |
| Enquadramento em tempo real no formulário (`hfPseudo`) | ✅ | Mandato (prévia do core) e boleta (prévia dos desvios no formulário; o core decide). |
| Cockpit (semáforo, funil produção → frete, exceções) | ❌ | A Home tem dashboards configuráveis. |
| Parâmetros da política: contingência, recompra (110%/10 d.u.), estresse, percentis quente/frio, mix, controles (confDU, regD, reporteH) | ✅ | `PolicyLimits` com os 26 parâmetros e os padrões do PDF (políticas existentes migradas com eles). Ainda não são lidos por motores que dependem de produção/mercado. |

## 6. Fora do escopo desta fase (o próprio PDF deixa para depois)

Feeds de mercado (ICE/Barchart, CEPEA, ANP, PTAX), integração ERP, leitura automática de contratos anexados, alertas e
relatório de fechamento. Decisões **provisórias** do cliente (carry × expira, opção sem exercício, CFaR, alçada de exceção)
não devem ser "resolvidas" sozinhas.
