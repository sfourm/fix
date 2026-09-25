# Contexto do repositório Fix

Leitura obrigatória antes de alterar código (pessoas e assistentes de IA). Esta pasta descreve o produto, a arquitetura de
cada projeto e os padrões que o código já segue. Ao mudar uma regra ou convenção descrita aqui, atualize o arquivo
correspondente no mesmo PR.

## Índice

| Arquivo | Conteúdo |
| --- | --- |
| [produto.md](produto.md) | O que é o Fix, fluxo do FIX2, entidades do negócio e glossário |
| [acesso-e-permissoes.md](acesso-e-permissoes.md) | Multitenancy, organização interna FIX, owner/user, alçadas, roles de decisão e organograma |
| [arquitetura.md](arquitetura.md) | Visão do sistema: aplicações, responsabilidades, fluxo de um request e decisões de arquitetura |
| [core-service.md](core-service.md) | Backend .NET: camadas, fluxo de caso de uso, padrões de domínio, persistência, auditoria e erros |
| [contratos-grpc.md](contratos-grpc.md) | `protos/`: organização, regras e como evoluir contratos |
| [bff.md](bff.md) | BFF Node.js: camadas, autenticação, dashboards/filtros/pesquisa, cache e integração gRPC |
| [web.md](web.md) | Web Vue 3: camadas, navegação, permissões, componentes e gráficos |
| [observabilidade.md](observabilidade.md) | OpenTelemetry, traces, métricas, dashboards do Grafana e como instrumentar |
| [infraestrutura.md](infraestrutura.md) | Cluster k3s na AWS (Terraform), imagens, chart Helm, ingress-nginx, pipelines do GitHub e acesso pelo kubectl |
| [desenvolvimento.md](desenvolvimento.md) | Como rodar, testes, receita de funcionalidade ponta a ponta, convenções de trabalho e armadilhas |

## Resumo em uma tela

- **Produto**: plataforma multitenant de gestão de riscos de commodities (modelo FIX2):
  `Setup da companhia → Política de riscos → Mandatos → Boletas de hedge → Confirmation`.
- **Aplicações**: `frontend/web` (Vue) → `frontend/bff` (Node, REST) → `backend/core-service` (.NET, gRPC) → PostgreSQL.
  O BFF também usa Elasticsearch (dashboards e filtros) e Redis (cache). Tudo emite telemetria para `observability/`.
- **Regra de ouro**: regra de negócio e autorização ficam no **core**; autenticação, tenant do request e preferências de
  visualização (dashboards, filtros, pesquisa) ficam no **BFF**; o **web** só apresenta.
- **Idioma**: produto e mensagens em português do Brasil; código e identificadores em inglês; comentários e docs em português.
