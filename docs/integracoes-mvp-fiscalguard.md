# FiscalGuard Tech - Integrações do MVP

Este documento resume as integrações utilizadas no MVP do FiscalGuard Tech e explica o papel de cada uma na proposta de valor do produto.

O objetivo do MVP não é substituir sistemas oficiais nem consultar dados protegidos por autenticação governamental. O objetivo é demonstrar, de forma realista e verificável, que o FiscalGuard consegue transformar dados públicos e sinais operacionais em priorização, alertas e recomendações úteis para escritórios contábeis.

## Visão Executiva

O FiscalGuard Tech integra fontes públicas e serviços operacionais para responder a três perguntas centrais:

1. **Minha carteira de CNPJs está regular?**
2. **Quais clientes exigem atenção primeiro?**
3. **Que ação o contador deve tomar antes que o problema cresça?**

No MVP, as integrações permitem demonstrar o fluxo:

**Cadastrar ou importar carteira → consultar dados públicos → classificar risco → gerar alertas e recomendações → manter histórico → orientar o contador.**

## 1. BrasilAPI CNPJ

**Serviço utilizado:** BrasilAPI - endpoint público de CNPJ  
**Tipo de integração:** API HTTP pública  
**Chave de API:** Não exige chave  
**Custo:** Gratuito  
**Uso no FiscalGuard:** Consulta cadastral e enriquecimento de empresas

### Para Que Serve

A integração com a BrasilAPI permite consultar dados públicos vinculados a um CNPJ, como:

- razão social;
- nome fantasia, quando disponível;
- situação cadastral;
- CNAE principal;
- endereço público;
- indicador público de opção pelo Simples Nacional, quando disponível na fonte;
- indicador público de MEI, quando disponível na fonte.

### Valor Para o Escritório Contábil

Essa integração reduz o trabalho manual de conferência cadastral e permite que o escritório visualize rapidamente se uma empresa cadastrada na carteira possui sinais que exigem atenção.

Exemplos de sinais tratados:

- empresa baixada, inapta, suspensa ou em situação cadastral sensível;
- empresa não marcada como optante pelo Simples Nacional;
- ausência de CNAE principal;
- dados públicos incompletos ou indisponíveis.

### Valor Para Investidores

A integração demonstra que o produto já opera sobre dados reais e não depende apenas de cadastros manuais ou dados simulados. Isso aumenta a percepção de maturidade do MVP e reforça a tese de automação preventiva para escritórios contábeis.

### Limitações

A BrasilAPI consolida dados públicos e pode sofrer indisponibilidade, defasagem ou ausência de campos. Ela não substitui a validação oficial diretamente nos portais governamentais.

Também não consulta débitos fiscais, DAS em aberto, parcelamentos ou pendências protegidas por autenticação.

## 2. Receita Federal - Notícias Oficiais via RSS

**Serviço utilizado:** RSS oficial de notícias da Receita Federal  
**Tipo de integração:** Feed RSS/XML público  
**Chave de API:** Não exige chave  
**Custo:** Gratuito  
**Uso no FiscalGuard:** Radar fiscal e regulatório

### Para Que Serve

Essa integração permite acompanhar publicações oficiais da Receita Federal relacionadas a temas fiscais, cadastrais e operacionais.

O FiscalGuard usa esse feed para identificar publicações com termos relevantes, como:

- CNPJ;
- obrigações;
- prazos;
- DCTFWeb;
- EFD-Reinf;
- SPED;
- documentos fiscais;
- CBS/IBS;
- reforma tributária;
- indisponibilidade de sistemas;
- orientações oficiais.

### Valor Para o Escritório Contábil

O contador passa a ter um radar centralizado de publicações oficiais que podem afetar sua carteira de clientes. Em vez de depender de acompanhamento manual em múltiplos portais, o FiscalGuard traz o sinal para dentro do fluxo de trabalho.

### Valor Para Investidores

Essa integração aumenta a diferenciação do produto. O FiscalGuard deixa de ser apenas uma tela de cadastro de CNPJ e passa a funcionar como uma camada de inteligência operacional sobre fontes oficiais.

### Limitações

O feed RSS informa publicações oficiais, mas o MVP ainda não interpreta juridicamente todo o conteúdo. A classificação atual é feita por motor de regras e palavras-chave, com recomendação conservadora quando o tema exige análise técnica.

## 3. Receita Federal / Simples Nacional via RSS

**Serviço utilizado:** Feed RSS da seção Receita Federal / Simples Nacional  
**Tipo de integração:** Feed RSS/XML público  
**Chave de API:** Não exige chave  
**Custo:** Gratuito  
**Uso no FiscalGuard:** Radar específico para empresas do Simples, MEI e pequeno porte

### Para Que Serve

Essa integração monitora publicações relacionadas ao público-alvo mais importante do MVP: pequenas empresas, MEIs e clientes do Simples Nacional.

O FiscalGuard usa essa fonte para identificar temas como:

- Simples Nacional;
- MEI;
- microempresa;
- empresa de pequeno porte;
- exclusão ou regularização;
- parcelamento;
- prazos;
- mudanças em regras ou orientações oficiais.

### Valor Para o Escritório Contábil

Para escritórios que atendem PMEs, esse radar é mais próximo da rotina diária do contador. Ele ajuda a antecipar conversas com clientes antes que uma mudança oficial se transforme em urgência operacional.

### Valor Para Investidores

Mostra foco em um nicho claro e comercialmente relevante: escritórios contábeis com carteiras de pequenas empresas. Esse recorte reduz dispersão e melhora a clareza da proposta de valor.

### Limitações

Nem toda informação sobre Simples Nacional aparece em feed estruturado. Algumas informações podem exigir acompanhamento adicional em portais específicos ou integração futura com fontes oficiais mais detalhadas.

## 4. Câmara dos Deputados - Dados Abertos

**Serviço utilizado:** Dados Abertos da Câmara dos Deputados  
**Tipo de integração:** API pública  
**Chave de API:** Não exige chave  
**Custo:** Gratuito  
**Uso no FiscalGuard:** Radar legislativo e regulatório

### Para Que Serve

A integração com a Câmara dos Deputados permite acompanhar proposições legislativas relacionadas a temas fiscais e empresariais.

O FiscalGuard monitora temas como:

- Simples Nacional;
- MEI;
- microempresa;
- empresa de pequeno porte;
- tributação;
- nota fiscal;
- ICMS;
- ISS;
- CBS/IBS;
- reforma tributária.

### Valor Para o Escritório Contábil

Projetos de lei não geram obrigação imediata, mas ajudam o contador a enxergar tendências regulatórias. O valor está em antecipar assuntos que podem afetar clientes no médio prazo.

### Valor Para Investidores

A integração amplia a visão do produto: além de olhar para a situação atual da empresa, o FiscalGuard começa a monitorar mudanças em discussão no ambiente regulatório.

### Limitações

Proposições legislativas são sinais estratégicos, não obrigações fiscais vigentes. Por isso, o produto diferencia esse tipo de alerta de publicações oficiais já efetivas.

## 5. Motor de Classificação e Recomendações

**Serviço utilizado:** Motor interno de regras  
**Tipo de integração:** Lógica própria do FiscalGuard  
**Chave de API:** Não se aplica  
**Custo:** Não se aplica  
**Uso no FiscalGuard:** Conversão de sinais em risco, impacto e ação sugerida

### Para Que Serve

O motor de regras transforma dados vindos das integrações em informações acionáveis:

- severidade;
- score de risco;
- resumo de risco;
- pendência fiscal;
- recomendação ao contador;
- impacto prático;
- empresa potencialmente afetada por uma publicação oficial.

### Valor Para o Escritório Contábil

O contador não recebe apenas um dado bruto. Ele recebe uma leitura operacional:

- o que foi detectado;
- por que importa;
- qual próxima ação tomar;
- qual cliente deve ser priorizado.

### Valor Para Investidores

Essa camada é o início do ativo de produto do FiscalGuard. As integrações coletam dados, mas o motor de regras transforma esses dados em fluxo de trabalho e valor percebido.

### Observação Importante

No MVP, esse motor é baseado em regras auditáveis, não em IA generativa. Isso é intencional: em contexto fiscal, previsibilidade e explicabilidade são mais importantes do que respostas probabilísticas.

## 6. Importação de Carteira por CSV

**Serviço utilizado:** Upload de CSV no próprio sistema  
**Tipo de integração:** Importação de arquivo  
**Chave de API:** Não se aplica  
**Custo:** Não se aplica  
**Uso no FiscalGuard:** Entrada rápida de múltiplos CNPJs

### Para Que Serve

Permite que um escritório importe vários CNPJs de uma só vez, acelerando a criação da carteira monitorada.

O importador trata:

- CNPJs válidos;
- CNPJs inválidos;
- duplicidades;
- limite do plano;
- resultado linha a linha.

### Valor Para o Escritório Contábil

Sem importação em massa, o MVP pareceria viável apenas para poucos clientes. Com CSV, o fluxo se aproxima da realidade de escritórios que gerenciam dezenas ou centenas de empresas.

### Valor Para Investidores

Demonstra potencial de escala operacional: o produto não depende de cadastro manual individual para gerar valor.

### Limitações

O CSV exige padronização mínima das colunas. Integrações futuras podem incluir Excel, Google Sheets ou sistemas contábeis.

## 7. SMTP para Envio Real de E-mails

**Serviço utilizado:** Servidor SMTP configurável  
**Tipo de integração:** E-mail transacional  
**Chave de API:** Depende do provedor utilizado  
**Custo:** Depende do provedor; pode usar serviços com free tier  
**Uso no FiscalGuard:** Envio real de alertas por e-mail

### Para Que Serve

Permite que alertas gerados pelo FiscalGuard sejam enviados por e-mail quando houver configuração SMTP.

No ambiente de desenvolvimento, se não houver SMTP configurado, o sistema registra o envio em log usando um serviço de desenvolvimento.

### Valor Para o Escritório Contábil

Alertas deixam de ser apenas informação dentro do painel e passam a chegar ao responsável, aumentando a chance de ação preventiva.

### Valor Para Investidores

Demonstra que o produto já possui caminho para comunicação ativa, essencial para retenção e valor recorrente.

### Limitações

O MVP ainda não inclui gestão avançada de templates, preferências de notificação por cliente ou múltiplos canais como WhatsApp/Telegram.

## 8. Monitoramento Periódico com Quartz

**Serviço utilizado:** Quartz Scheduler  
**Tipo de integração:** Agendamento interno de jobs  
**Chave de API:** Não se aplica  
**Custo:** Gratuito/open source  
**Uso no FiscalGuard:** Execução periódica de consultas fiscais

### Para Que Serve

O Quartz permite executar rotinas automáticas em intervalo configurável, como monitorar empresas com consulta vencida ou próxima consulta programada.

### Valor Para o Escritório Contábil

O escritório não precisa lembrar manualmente de consultar cada CNPJ. A carteira passa a ter rotina de acompanhamento recorrente.

### Valor Para Investidores

Essa integração reforça a natureza SaaS do produto: o FiscalGuard não é apenas uma consulta pontual, mas uma operação contínua de monitoramento.

### Limitações

No MVP, a profundidade do monitoramento depende das fontes públicas disponíveis. Débitos protegidos por autenticação ainda exigem integrações futuras com autorização, certificado digital ou APIs oficiais.

## Integrações Não Implementadas no MVP

Algumas integrações possuem alto valor, mas não foram tratadas como implementação principal do MVP por exigirem autenticação, convênios, certificado digital ou maior complexidade operacional.

Exemplos:

- consulta direta a débitos fiscais protegidos;
- DAS em aberto com autenticação oficial;
- parcelamentos detalhados;
- procuração eletrônica;
- certificado digital;
- integração completa com prefeituras e SEFAZ estaduais;
- WhatsApp oficial;
- integração com ERPs ou sistemas contábeis.

Essas integrações são parte natural do roadmap, mas o MVP atual evita simular dados protegidos como se fossem consultas reais.

## Mensagem Para Apresentação

O FiscalGuard Tech já demonstra um fluxo realista e defensável:

> O escritório importa sua carteira, o sistema consulta dados públicos, classifica risco, cruza publicações oficiais com os clientes e gera recomendações operacionais para orientar a equipe contábil.

O diferencial do MVP não está apenas em consultar CNPJ. Está em transformar sinais dispersos em uma fila clara de ação preventiva.

