# CatalogAPI (Fase 3 - Tech Challenge)

Este repositório contém o microsserviço de Catálogo de Jogos do ecossistema FiapCloudGames.

## 🚀 Novidades da Fase 3
- **Migração para MongoDB:** O banco relacional foi substituído pelo MongoDB (NoSQL), garantindo alta escalabilidade na leitura do catálogo.
- **Cache Distribuído com Redis:** As requisições de listagem de jogos agora passam por um cache super-rápido no Redis, reduzindo drasticamente a carga sobre o MongoDB e o tempo de resposta.
- **Observabilidade (OpenTelemetry):** Instrumentação nativa em C# para geração de métricas de telemetria lidas pelo **Prometheus**.
- **Integração Serverless (SQS/Lambda):** A publicação de eventos de notificação abandonou a mensageria simples e foi refatorada para acionar funções na Nuvem AWS.
