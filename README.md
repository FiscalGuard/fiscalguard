# FiscalGuard Tech

SaaS multi-tenant para monitoramento fiscal preventivo de escritorios contabeis.

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

As rotas publicas foram preservadas e ampliadas (`/api/auth`, `/api/dashboard`, `/api/companies`, `/api/issues`, `/api/alerts`, `/api/plans`, `/api/users`, `/api/subscription`, `/api/organization`) para o frontend consumir os modulos operacionais.

## Funcionalidades implementadas nesta etapa

- Cadastro de organizacao e primeiro usuario proprietario.
- Login, refresh token revogavel e JWT com `organization_id`.
- Entidades principais do dominio multi-tenant.
- EF Core com PostgreSQL, indices e migration inicial.
- Planos Trial, Gratuito, Profissional e Escritorio.
- Validacao centralizada de limite de CNPJs.
- Cadastro/listagem de empresas com validacao de CNPJ e duplicidade por organizacao.
- Provider fiscal isolado por interface, atualmente com implementacao local previsivel para desenvolvimento.
- Consulta manual, historico, pendencias deduplicadas, alertas internos e notificacoes.
- Quartz configurado com job nao concorrente para processamento automatico em lote conforme plano.
- Health checks e Swagger.
- Frontend React/TypeScript/Tailwind com rotas principais.
- Testes para CNPJ, limite de plano e deduplicacao de pendencias.
- Gestao basica de usuarios da organizacao com convites, perfis, status e bloqueio do ultimo proprietario.
- Tela e API de assinatura com troca controlada de plano conforme uso atual.
- Tela e API de configuracoes da organizacao.
- Detalhamento de empresa com dados cadastrais, situacao fiscal, consultas e pendencias.
- Historico de consultas fiscais por empresa.
- Alteracao de status de pendencias com historico e auditoria.
- Provider real de CNPJ via BrasilAPI, com situacao cadastral, CNAE, endereco e indicador de Simples/MEI quando disponivel na fonte publica.
- Enriquecimento cadastral da empresa apos consulta fiscal.
- Score de risco fiscal explicavel por empresa, com nivel Baixo, Medio, Alto ou Critico.
- Recomendacoes automaticas associadas as pendencias encontradas.
- Importacao de carteira por CSV em `/api/companies/import`.
- Dashboard executivo com maiores riscos da carteira, falhas de consulta e empresas sem primeira consulta.
- Radar fiscal e regulatório com RSS oficial da Receita Federal/Simples Nacional e Dados Abertos da Câmara dos Deputados.
- Classificação explicável de alertas por impacto operacional, tema, fonte e empresas potencialmente afetadas na carteira.

## Modelo de dados proposto

Inclui `Organization`, `AppUser`, `OrganizationUser`, `Customer`, `Company`, `FiscalConsultation`, `FiscalConsultationItem`, `FiscalStatusHistory`, `FiscalIssue`, `FiscalIssueHistory`, `Alert`, `Notification`, `Plan`, `Subscription`, `SubscriptionUsage`, `JobExecution`, `AuditLog`, `RefreshToken` e `UserInvitation`.

Todos os dados de negocio possuem `OrganizationId`, e os servicos usam o tenant do JWT, nao do front-end, para consultas e gravacoes.

## Lacunas frente ao escopo completo

- Recuperacao de senha e aceite real de convite ainda estao estruturados, mas nao completos.
- Pagamento real nao foi integrado por decisao de MVP.
- Pendencias fiscais protegidas por autenticacao governamental continuam fora do MVP; o produto usa dados publicos reais e sinais derivados, sem simular consulta autenticada como se fosse real.
- E-mail esta em modo desenvolvimento, com registros de notificacao persistidos no banco.
- Relatorios em PDF, e-mail transacional real e visualizacao detalhada de auditoria devem entrar nas proximas etapas.

## Como rodar

1. Suba o PostgreSQL:

```bash
docker compose up -d
```

2. Rode a API:

```bash
dotnet ef database update --project src/FiscalGuard.Infrastructure --startup-project src/FiscalGuard.Api
dotnet run --project src/FiscalGuard.Api
```

3. Rode o frontend:

```bash
cd frontend
npm install
npm run dev
```

Configuracao fiscal padrao:

```text
FiscalData__Provider=BrasilApi
FiscalData__BrasilApiBaseUrl=https://brasilapi.com.br/api/
```

Para desenvolvimento sem chamadas externas, use `FiscalData__Provider=Mock`.

Radar fiscal e regulatório:

```text
RegulatoryRadar__CamaraBaseUrl=https://dadosabertos.camara.leg.br/api/v2/
RegulatoryRadar__LookbackDays=1460
RegulatoryRadar__MaxItems=8
RegulatoryRadar__CacheMinutes=30
RegulatoryRadar__OfficialFeeds__0__Name=Receita Federal - Noticias oficiais
RegulatoryRadar__OfficialFeeds__0__Url=https://www.gov.br/receitafederal/pt-br/assuntos/noticias/RSS
RegulatoryRadar__OfficialFeeds__0__Theme=receita federal
RegulatoryRadar__OfficialFeeds__1__Name=Receita Federal / Simples Nacional
RegulatoryRadar__OfficialFeeds__1__Url=https://www.gov.br/receitafederal/simples/pt-br/assuntos/noticias/RSS
RegulatoryRadar__OfficialFeeds__1__Theme=simples nacional
```

O radar usa cache em memoria por `RegulatoryRadar__CacheMinutes`; entrar na tela novamente dentro desse intervalo reutiliza o resultado e apenas refaz o cruzamento com a carteira do tenant. As fontes atuais nao exigem chave de API.

E-mail real e opcional por SMTP:

```text
Smtp__Host=smtp.seuprovedor.com
Smtp__Port=587
Smtp__EnableSsl=true
Smtp__From=alertas@seudominio.com
Smtp__Username=usuario
Smtp__Password=senha-ou-token
```

Sem `Smtp__Host`, o sistema usa `DevelopmentEmailService` e apenas registra o envio em log.

CSV de importacao aceito:

```csv
cnpj,razao_social,nome_fantasia,email,responsavel,tags
19131243000197,Empresa Exemplo,Exemplo,cliente@exemplo.com,Ana Silva,prioritario
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
2. Etapa 2: completar organizacoes, usuarios, empresas, planos e trial. Concluida parcialmente com APIs e telas de organizacao, usuarios e assinatura.
3. Etapa 3: ampliar consultas fiscais, normalizacao, pendencias e historico. Concluida parcialmente com detalhe de empresa, historico de consultas e fluxo de status de pendencias.
4. Etapa 4: alertas, notificacoes e Quartz operacional em lote. Concluida parcialmente com notificacoes internas, e-mail de desenvolvimento e job automatico.
5. Etapa 5: provider fiscal externo real, e-mail real configuravel, dashboard refinado, testes de integracao e documentacao de operacao.
