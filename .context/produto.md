# Produto

## O que é

O Fix é uma plataforma **multitenant** de gestão de riscos de commodities, baseada no modelo **FIX2** (protótipo funcional
de telas e regras em `example/FIX2_v60.html`). Cada organização cliente (usina, trading, produtor) usa o sistema para definir
como pode se proteger (hedge) e para registrar, aprovar e conferir as operações feitas.

A equipe interna da FIX usa a mesma plataforma para apoiar os clientes (ver [acesso-e-permissoes.md](acesso-e-permissoes.md)).

## Fluxo do negócio

```
Setup da companhia → Política de riscos (aprovada em ata) → Mandatos (autorizam volume num eixo) → Boletas de hedge → Confirmation
```

1. **Setup da companhia**: identificação (razão social, CNPJ, setor, safra ativa), capacidade industrial, orçamento
   (custo caixa, piso econômico — gatilhos de fixação), financeiro, commodities negociadas, membros e mesas.
2. **Política de riscos**: documento-mãe versionado que define o que a companhia pode fazer.
   - Ciclo: `Draft (Rascunho) → UnderApproval (Em aprovação) → Active (Vigente) → Superseded (Substituída)`.
   - Só rascunho/em aprovação é editável; aprovar exige ata e substitui a vigente. Mudança em política vigente = nova versão.
   - Conteúdo: limites (§6–§8), **eixos** por fator de risco, **bandas de cobertura** por safra, **instrumentos permitidos**.
3. **Mandato**: autorização de volume, preço e janela num eixo da política; tem consumido e saldo.
   - O tipo do mandato precisa ser compatível com o fator de risco do eixo.
   - **Enquadramento automático**: vigência, horizonte, janela, piso econômico/gatilho.
   - Dentro da política + role `self_approve` → ativo direto; senão vai para a **fila de aprovação**.
   - Fora da política só aprova quem tem `approve_exception`.
4. **Boleta de hedge** (futuro, opção ou NDF) sobre um mandato ativo.
   - Consome saldo do mandato **somente quando aprovada**; não pode exceder o saldo.
   - Só contrapartes **homologadas** aceitam boletas.
5. **Confirmation** (middle office): `Pending → Confirmed | Divergent | Refused`; atrasada após 2 dias úteis.
   Tela "Boletas em aberto" lista pendentes/divergentes/recusadas.

Transversais:
- **Contrapartes**: cadastro, tipo, limites nocional e MtM (US$), homologação. Com boletas vinculadas não podem ser excluídas
  (desomologar bloqueia novas operações).
- **Fila de aprovação**: mandatos e boletas aguardando decisão de quem está acima no organograma.
- **Timeline**: auditoria de toda alteração (quem, quando, valores antigos e novos), por agregado.
- **Home**: dashboards personalizáveis (privados ou públicos para a organização) e filtros salvos (ver [bff.md](bff.md)).

## Entidades principais (agregados do core)

| Agregado | Guarda | Regras-chave |
| --- | --- | --- |
| Organization | Perfil, capacidade, orçamento, financeiro, commodities, membros (com mesa), grupos (organograma) | Sempre um owner; grupo raiz "Direção" |
| Counterparty | Contraparte, tipo, limites, homologação | Só homologada aceita boleta |
| Policy | Versões, limites, eixos, bandas, instrumentos, histórico | Só rascunho/em aprovação é editável |
| Mandate | Termos, eixo, status, consumido/saldo | Enquadramento; aprovação conforme alçada e organograma |
| Order (boleta) | Tipo, direção, quantidade, preço, contraparte, aprovação, confirmation | Consome saldo ao aprovar |
| Role / Rule | Permissões atômicas / conjuntos atribuíveis (alçadas) | Ver [acesso-e-permissoes.md](acesso-e-permissoes.md) |

## Glossário

| Termo | Significado |
| --- | --- |
| **Commodities** | Açúcar VHP (NY11), açúcar branco (Londres nº5), etanol hidratado, etanol anidro, milho (CBOT), soja (CBOT) |
| **Setor** | Sucroenergético, grãos, pecuária |
| **Mesa (desk)** | Mesa de execução, comercial, logística, diretoria, controle de riscos |
| **Fator de risco** | Físico, preço, moeda, frete — cada eixo da política cobre um |
| **Eixo** | Linha da política que limita a exposição num fator de risco; mandatos são emitidos sobre eixos |
| **Banda de cobertura** | Faixa mínima/máxima de hedge por safra |
| **Instrumento** | Permitido, com teto ou vedado, por tipo de operação |
| **Tipo de mandato** | Precificação, moeda, comercial, logística |
| **Boleta** | Registro de uma operação de hedge: futuro, opção ou NDF |
| **NDF** | Non-deliverable forward (derivativo de moeda liquidado pela diferença) |
| **MtM** | Mark-to-market: exposição a valor de mercado, usada nos limites de contraparte |
| **Ata** | Registro da aprovação formal da política |
| **Alçada** | Conjunto de permissões atribuído a membros/grupos; a "alçada de emissão" é a role `self_approve` |
| **Confirmation** | Conferência da boleta com a contraparte pelo middle office |
| **Safra** | Ano-safra (ex.: 26/27) usado em bandas e na safra ativa |
| **Unidades** | Lotes, toneladas, sacas, m³, libras, US$ |

## Linguagem da interface

Português do Brasil em toda a UI e em mensagens de erro de negócio (as `DomainException` do core já trazem o texto final
para o usuário). Rótulos de enums ficam centralizados no web em `frontend/web/src/domain/labels.ts`.
