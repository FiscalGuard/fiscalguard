# FiscalGuard Tech

MVP SaaS multi-tenant para monitoramento fiscal preventivo de escritorios contabeis.

## Diagnostico inicial

- Estrutura encontrada: o workspace estava vazio e nao era um repositorio Git.
- Tecnologias encontradas no ambiente: .NET SDK 8.0.400 e Node.js 24.11.1.
- Materiais analisados: `FiscalGuard Tech - TB revisão.docx`, `FiscalGuard_Tech_Overview.pptx`, imagens de logotipo e o pedido anexado.
- Ponto de referencia do negocio: foco B2B em escritorios contabeis, trial com analise inicial, linguagem acessivel e cuidado para nao prometer garantia de regularidade fiscal.
- Figma: o link publico informado nao ficou acessivel por busca/navegacao nesta sessao; a identidade visual foi baseada nos assets locais fornecidos.

## Estrutura criada

```text
src/
  FiscalGuard.Api
    Controllers/
    Security/
  FiscalGuard.Application
    DTOs/
    Interfaces/
  FiscalGuard.Domain
    Abstractions/
    Entities/
    Enums/
  FiscalGuard.Infrastructure
    Jobs/
    Notifications/
    Providers/
    Seed/
    Services/
tests/
  FiscalGuard.Tests
frontend/
  src/
```

## Padrao arquitetural do backend

API com controllers finos, dominio com entidades e enums, contratos de aplicacao em DTOs/interfaces e infraestrutura com EF Core, servicos concretos, providers, notificacoes, seed e jobs.

As rotas publicas foram preservadas (`/api/auth`, `/api/dashboard`, `/api/companies`, `/api/issues`, `/api/alerts`, `/api/plans`) para o frontend continuar funcionando sem ajuste.

## Funcionalidades implementadas nesta etapa

- Cadastro de organizacao e primeiro usuario proprietario.
- Login, refresh token revogavel e JWT com `organization_id`.
- Entidades principais do dominio multi-tenant.
- EF Core com PostgreSQL, indices e migration inicial.
- Planos Trial, Gratuito, Profissional e Escritorio.
- Validacao centralizada de limite de CNPJs.
- Cadastro/listagem de empresas com validacao de CNPJ e duplicidade por organizacao.
- Provider fiscal mock com cenarios previsiveis.
- Consulta manual simulada, historico, pendencias deduplicadas e alertas internos.
- Quartz configurado com job nao concorrente para selecao em lotes.
- Health checks e Swagger.
- Frontend React/TypeScript/Tailwind com rotas principais.
- Testes para CNPJ, limite de plano e deduplicacao de pendencias.

## Modelo de dados proposto

Inclui `Organization`, `AppUser`, `OrganizationUser`, `Customer`, `Company`, `FiscalConsultation`, `FiscalConsultationItem`, `FiscalStatusHistory`, `FiscalIssue`, `FiscalIssueHistory`, `Alert`, `Notification`, `Plan`, `Subscription`, `SubscriptionUsage`, `JobExecution`, `AuditLog`, `RefreshToken` e `UserInvitation`.

Todos os dados de negocio possuem `OrganizationId`, e os servicos usam o tenant do JWT, nao do front-end, para consultas e gravacoes.

## Lacunas frente ao escopo completo

- Convites de usuarios, recuperacao de senha e tela detalhada de usuario ainda estao estruturados, mas nao completos.
- Pagamento real nao foi integrado por decisao de MVP.
- Provider fiscal externo real nao foi implementado; ha apenas `MockFiscalDataProvider`.
- E-mail esta em modo desenvolvimento.
- Acoes em lote, relatorios e historico visual completo devem entrar nas proximas etapas.

## Como rodar

1. Suba o PostgreSQL:

```bash
docker compose up -d
```

2. Rode a API:

```bash
dotnet run --project src/FiscalGuard.Api
```

3. Rode o frontend:

```bash
cd frontend
npm install
npm run dev
```

Credenciais seed:

```text
demo@fiscalguard.local
Demo@12345
```

## Validacao

```bash
dotnet build FiscalGuard.sln
dotnet test FiscalGuard.sln
cd frontend
npm run build
```

## Plano incremental

1. Etapa 1: estrutura, autenticacao, multi-tenancy, banco e migrations. Concluida.
2. Etapa 2: completar organizacoes, usuarios, empresas, planos e trial.
3. Etapa 3: ampliar consultas fiscais, normalizacao, pendencias e historico.
4. Etapa 4: alertas, notificacoes, e-mail real configuravel e Quartz operacional em lote.
5. Etapa 5: dashboard refinado, UX final, testes de integracao e documentacao de operacao.
