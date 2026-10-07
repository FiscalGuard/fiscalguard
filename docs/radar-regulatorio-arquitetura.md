# FiscalGuard Tech - Arquitetura do Radar Fiscal e Regulatório

## Objetivo

O radar regulatório deixa de ser apenas uma tela de notícias e passa a funcionar como um motor auditável de inteligência fiscal. A proposta é unir fontes oficiais, evidências públicas, perfil da carteira e regras explicáveis para responder três perguntas:

1. Qual fato oficial foi identificado?
2. Quais empresas da carteira podem ser impactadas?
3. Que ação preventiva o escritório contábil deve priorizar?

## Entrega implementada nesta etapa

### 1. Diagnóstico das fontes

Cada atualização do radar registra a execução por fonte consultada:

- fonte;
- sucesso ou falha;
- quantidade de documentos lidos;
- quantidade de documentos aceitos pelo filtro fiscal;
- tempo de resposta;
- mensagem técnica ou erro.

Isso evita a tela vazia sem explicação. Se o radar não trouxer alertas, o usuário consegue saber se as fontes falharam, se responderam sem documentos ou se os documentos foram descartados pelos filtros.

### 2. Persistência das evidências

As publicações normalizadas agora são salvas em `RegulatoryDocuments`. O registro guarda:

- identificador externo;
- fonte oficial;
- título;
- resumo;
- URL pública;
- tema fiscal;
- nível de impacto;
- justificativa;
- impacto para o cliente;
- ação sugerida;
- texto pesquisável;
- data de publicação.

Isso permite histórico, auditoria e futuras telas de busca sem depender apenas da memória/cache.

### 3. Cruzamento com a carteira

O motor cria cruzamentos em `RegulatoryMatches`, relacionando documento oficial e empresa impactada. O score inicial é baseado em regras auditáveis:

- regime tributário;
- indício de Simples Nacional;
- indício de MEI;
- CNAE principal;
- tags internas da empresa;
- temas fiscais amplos.

Cada cruzamento retorna motivo, termos encontrados e nota de aderência.

### 4. Correções de robustez nas integrações

O leitor de RSS foi ajustado para suportar feeds com namespaces diferentes, e a consulta da Câmara dos Deputados força resposta JSON. Isso reduz casos em que o navegador mostra XML ou o feed muda levemente o formato.

### 5. Experiência na tela

A tela do radar passa a exibir:

- indicadores de documentos lidos e aceitos;
- evidências salvas;
- cruzamentos atuais e históricos;
- diagnóstico por fonte;
- explicação do modelo de matching;
- score de aderência por empresa.

## Próximas etapas recomendadas

1. Adicionar coletores para novas fontes oficiais, começando por Senado Federal e Diário Oficial da União quando houver configuração válida de acesso.
2. Criar cadastro de taxonomia fiscal, com sinônimos por tema e impacto por perfil de empresa.
3. Evoluir tags da empresa para categorias operacionais: comércio, serviço, indústria, saúde, educação, alimentação, construção, tecnologia, transporte e outros.
4. Criar tela de "Fontes e cobertura" para mostrar a saúde das integrações.
5. Adicionar IA como camada assistida para resumir publicações e sugerir tags, mantendo a fonte oficial e as regras como evidência principal.
